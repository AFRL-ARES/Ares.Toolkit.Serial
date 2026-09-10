using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Ares.Toolkit.Serial;

/// <summary>
/// Represents a physical serial port that may be shared by multiple
/// compatible AresHardwareConnection instances.
///
/// A SharedSerialPort owns the underlying System.IO.Ports.SerialPort.
/// Instances are acquired through Acquire() and automatically disposed
/// when the final attached hardware connection releases the port.
/// </summary>
internal sealed class SharedSerialPort : IDisposable
{
  private static readonly Dictionary<string, SharedSerialPort> Ports = new(StringComparer.OrdinalIgnoreCase);
  private static readonly object PortsLock = new();
  public event Action<byte[]>? DataReceived;
  private readonly object _stateLock = new();
  private readonly HashSet<AresHardwareConnection> _connections = [];
  private SerialPort? _systemPort;
  private bool _disposed;
  private readonly SemaphoreSlim _streamLock = new(1, 1);

  private SharedSerialPort(string portName, SerialPortConnectionInfo connectionInfo)
  {
    PortName = portName;
    ConnectionInfo = connectionInfo;
    Open();
  }

  internal Task AcquireStreamLock(CancellationToken token = default)
  {
    ThrowIfDisposed();
    return _streamLock.WaitAsync(token);
  }

  internal void ReleaseStreamLock()
  {
    _streamLock.Release();
  }

  /// <summary>
  /// Acquires a shared physical serial port for a hardware connection.
  ///
  /// If the port does not currently exist, it is created and opened.
  /// If it already exists, the requested connection must be compatible
  /// with the existing bus.
  /// </summary>
  public static SharedSerialPort Acquire(AresHardwareConnection connection, string portName, SerialPortConnectionInfo connectionInfo)
  {
    ArgumentNullException.ThrowIfNull(connection);
    ArgumentException.ThrowIfNullOrWhiteSpace(portName);
    ArgumentNullException.ThrowIfNull(connectionInfo);

    lock(PortsLock)
    {
      if(Ports.TryGetValue(portName, out var existing))
      {
        existing.AddConnection(connection, connectionInfo);
        return existing;
      }

      var sharedPort = new SharedSerialPort(portName, connectionInfo);
      sharedPort.AddConnection(connection, connectionInfo);
      Ports.Add(portName, sharedPort);

      return sharedPort;
    }
  }

  private void AddConnection(AresHardwareConnection connection, SerialPortConnectionInfo requestedInfo)
  {
    lock(_stateLock)
    {
      ThrowIfDisposed();

      if(_connections.Contains(connection))
          return;

      ValidateCompatibility(requestedInfo);

      _connections.Add(connection);
    }
  }

  /// <summary>
  /// Releases this physical serial port from a hardware connection.
  ///
  /// When the final hardware connection is removed, the physical
  /// port is closed, disposed, and removed from the registry.
  /// </summary>
  public void Release(AresHardwareConnection connection)
  {
    ArgumentNullException.ThrowIfNull(connection);

    lock(PortsLock)
    {
      bool shouldDispose;

      lock (_stateLock)
      {
          if (_disposed)
              return;

          _connections.Remove(connection);

          shouldDispose =
              _connections.Count == 0;
      }

      if (!shouldDispose)
          return;

      Ports.Remove(PortName);

      DisposeCore();
    }
  }

  private bool IsDedicatedProtocol(string protocol) => string.Equals(protocol, SerialDeviceProtocols.Dedicated, StringComparison.OrdinalIgnoreCase);

  private bool CompareProtocols(string protocol1, string protocol2) => string.Equals(protocol1, protocol2, StringComparison.OrdinalIgnoreCase);

  private void ValidateCompatibility(SerialPortConnectionInfo requested)
  {
    //A dedicated protocol implies that the port may not be shared.
    if(_connections.Count > 0 && IsDedicatedProtocol(ConnectionInfo.Protocol))
      throw new InvalidOperationException($"Serial port '{PortName}' is configured as a dedicated connection and cannot be shared.");


    if(!CompareProtocols(ConnectionInfo.Protocol, requested.Protocol))
    {
      throw new InvalidOperationException(
        $"Serial port '{PortName}' is already using protocol " +
        $"'{ConnectionInfo.Protocol}', but the requested device " +
        $"uses protocol '{requested.Protocol}'.");
    }


    if(ConnectionInfo.BaudRate != requested.BaudRate)
    {
      throw new InvalidOperationException(
        $"Serial port '{PortName}' is already configured for " +
        $"{ConnectionInfo.BaudRate} baud, but the requested " +
        $"device requires {requested.BaudRate} baud.");
    }


    if(ConnectionInfo.Parity != requested.Parity)
    {
      throw new InvalidOperationException(
        $"Serial port '{PortName}' is already configured with " +
        $"parity '{ConnectionInfo.Parity}', but the requested " +
        $"device requires '{requested.Parity}'.");
    }


    if(ConnectionInfo.DataBits != requested.DataBits)
    {
      throw new InvalidOperationException(
        $"Serial port '{PortName}' is already configured for " +
        $"{ConnectionInfo.DataBits} data bits, but the requested " +
        $"device requires {requested.DataBits}.");
    }


    if(ConnectionInfo.StopBits != requested.StopBits)
    {
      throw new InvalidOperationException(
        $"Serial port '{PortName}' is already configured for " +
        $"stop bits '{ConnectionInfo.StopBits}', but the requested " +
        $"device requires '{requested.StopBits}'.");
    }
  }

  public void Open()
  {
    lock (_stateLock)
    {
      ThrowIfDisposed();

      if(_systemPort?.IsOpen == true)
        return;

      if(_systemPort is null)
        _systemPort = CreateSystemPort();

      _systemPort.Open();
    }
  }


  public void Close()
  {
    lock (_stateLock)
    {
      ThrowIfDisposed();

      if(_systemPort is null) 
        return;

      if(_systemPort.IsOpen) 
        _systemPort.Close();

      _systemPort.DataReceived -= ProcessReceivedData;
      _systemPort.Dispose();
      _systemPort = null;
    }
  }


  /// <summary>
  /// Ensures that the physical serial port is open.
  /// If it has previously been closed, it is reopened.
  /// </summary>
  public void EnsureOpen()
  {
    lock (_stateLock)
    {
      ThrowIfDisposed();

      if(_systemPort?.IsOpen == true)
        return;

      if(_systemPort is null)
        _systemPort = CreateSystemPort();

      _systemPort.Open();
    }
  }


  /// <summary>
  /// Explicitly closes and reopens the underlying serial port.
  /// </summary>
  public void Reopen()
  {
    lock (_stateLock)
    {
      ThrowIfDisposed();

      if(_systemPort is not null)
      {
        if(_systemPort.IsOpen)
          _systemPort.Close();

        _systemPort.Dispose();
      }

      _systemPort = CreateSystemPort();

      _systemPort.Open();
    }
  }


  private SerialPort CreateSystemPort()
  {
    var port = new SerialPort(
        PortName,
        ConnectionInfo.BaudRate,
        ConnectionInfo.Parity,
        ConnectionInfo.DataBits,
        ConnectionInfo.StopBits
    )
    {
        DtrEnable = true,
        RtsEnable = true
    };


    port.DataReceived += ProcessReceivedData;

    return port;
  }

  private void ProcessReceivedData(object sender, SerialDataReceivedEventArgs e)
  {
    if(sender is not SerialPort port)
      return;

    var buffer = new byte[port.BytesToRead];

    if(buffer.Length == 0)
      return;

    var bytesRead = port.Read(buffer, 0, buffer.Length);

    if(bytesRead == 0)
      return;

    if(bytesRead != buffer.Length)
      Array.Resize(ref buffer, bytesRead);

    DataReceived?.Invoke(buffer);
  }

  public void Write(byte[] data, int offset, int count)
  {
    ArgumentNullException.ThrowIfNull(data);

    lock(_stateLock)
    {
      ThrowIfDisposed();

      if(_systemPort is null || !_systemPort.IsOpen)
        throw new InvalidOperationException($"Cannot write to serial port '{PortName}' because it is not open.");
      
      _systemPort.Write(data, offset, count);
    }
  }

  public void Dispose()
  {
    lock (PortsLock)
    {
      Ports.Remove(PortName);
      DisposeCore();
    }
  }

  private void DisposeCore()
  {
    lock (_stateLock)
    {
      if(_disposed)
        return;

      _disposed = true;

      _connections.Clear();

      if(_systemPort is not null)
      {
        if(_systemPort.IsOpen)
          _systemPort.Close();

        _systemPort.Dispose();
        _systemPort = null;
      }
    }
  }

  private void ThrowIfDisposed() 
    => ObjectDisposedException.ThrowIf(_disposed, this);

  public string PortName { get; }

  public SerialPortConnectionInfo ConnectionInfo { get; }

  public bool IsOpen
  {
    get
    {
      lock(_stateLock)
      {
        return _systemPort?.IsOpen ?? false;
      }
    }
  }

  public int ConnectionCount
  {
    get
    {
      lock(_stateLock)
      {
        return _connections.Count;
      }
    }
  }
}