using System;
using System.IO.Ports;

namespace Ares.Toolkit.Serial;

/// <summary>
/// A fluent builder for creating serial connections.
/// </summary>
public class SerialConnectionBuilder
{
  private string _portName = "COM1";
  private int _baudRate = 9600;
  private Parity _parity = Parity.None;
  private int _dataBits = 8;
  private StopBits _stopBits = StopBits.One;
  private readonly SerialConnectionOptions _options = new();

  public SerialConnectionBuilder WithPort(string portName)
  {
    _portName = portName;
    return this;
  }

  public SerialConnectionBuilder WithBaudRate(int baudRate)
  {
    _baudRate = baudRate;
    return this;
  }

  public SerialConnectionBuilder WithParity(Parity parity)
  {
    _parity = parity;
    return this;
  }

  public SerialConnectionBuilder WithDataBits(int dataBits)
  {
    _dataBits = dataBits;
    return this;
  }

  public SerialConnectionBuilder WithStopBits(StopBits stopBits)
  {
    _stopBits = stopBits;
    return this;
  }

  public SerialConnectionBuilder WithTimeout(TimeSpan timeout)
  {
    _options.SendTimeout = timeout;
    return this;
  }

  public IAresSerialConnection Build()
  {
    var info = new SerialPortConnectionInfo(_baudRate, _parity, _dataBits, _stopBits);
    return new AresHardwareConnection(info, _portName, _options);
  }
}
