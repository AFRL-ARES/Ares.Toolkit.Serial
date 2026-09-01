using Ares.Toolkit.Serial.Commands;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Ares.Toolkit.Serial;

public class AresHardwareConnection : AresSerialConnection
{
  public AresHardwareConnection(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, SerialConnectionOptions? options = null)
    : this(new SerialPortConnectionInfo(baudRate, parity, dataBits, stopBits), portName, options)
  {
  }

  public AresHardwareConnection(SerialPortConnectionInfo connectionInfo, 
    string portName, 
    SerialConnectionOptions? connectionOptions = null) : base(connectionInfo, portName, connectionOptions)
  {

  }

    //private SerialPort? SystemPort { get; set; }
    private SharedSerialPort? SharedPort { get; set; }

    protected override Task AcquireStreamLock(
    CancellationToken token = default)
    {
        if (SharedPort is null)
        {
            throw new InvalidOperationException(
                "Cannot acquire stream lock without an open shared serial port.");
        }

        return SharedPort.AcquireStreamLock(token);
    }

    protected override void ReleaseStreamLock()
    {
        if (SharedPort is null)
        {
            throw new InvalidOperationException(
                "Cannot release stream lock without an open shared serial port.");
        }

        SharedPort.ReleaseStreamLock();
    }

    protected override void Open(string portName)
    {
        if (SharedPort is not null && SharedPort.IsOpen)
        {
            IsOpen = true;
            return;
        }

        SharedPort = SharedSerialPort.Acquire(
            this,
            portName,
            ConnectionInfo
        );
        
        if (! SharedPort.IsOpen ) {SharedPort.Open(); }
        IsOpen = SharedPort.IsOpen;
    }

    protected override void CloseCore()
    {
        if (SharedPort is null)
            return;

        SharedPort.Release(this);
        SharedPort = null;
        IsOpen = false;
    }

    private void ProcessReceivedData(byte[] data)
    {
        AddDataReceived(data);
    }

    protected override void Listen()
  {
    if (SharedPort is null)
      throw new InvalidOperationException("Cannot listen on the hardware connection without first creating a port.");

    SharedPort.DataReceived += ProcessReceivedData;
  }

  protected override void StopListening()
  {
    if (SharedPort is null)
      throw new InvalidOperationException("Cannot stop listening on the hardware connection without first creating a port.");

        SharedPort.DataReceived -= ProcessReceivedData;
  }

  protected override void SendOutboundMessage(SerialCommand command)
  {
    if (!IsOpen || SharedPort is null)
      throw new InvalidOperationException("Cannot send message as the serial port is not open.");

    var serializedData = command.SerializedData;
        SharedPort.Write(serializedData, 0, serializedData.Length);
  }

}
