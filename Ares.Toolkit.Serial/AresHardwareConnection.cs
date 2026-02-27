using Ares.Toolkit.Serial.Commands;
using System;
using System.IO.Ports;

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

  private SerialPort? SystemPort { get; set; }

  protected override void Open(string portName)
  {
    // Make sure the port isn't already connected?
    if (SystemPort is not null && SystemPort.IsOpen)
      return;

    SystemPort = new SerialPort(
      portName,
      ConnectionInfo.BaudRate,
      ConnectionInfo.Parity,
      ConnectionInfo.DataBits,
      ConnectionInfo.StopBits
    );

    SystemPort.Open();
    IsOpen = SystemPort.IsOpen;
  }

  protected override void CloseCore()
  {
    if (SystemPort is null)
      return;

    var unopenedCopy = new SerialPort(
      SystemPort.PortName,
      SystemPort.BaudRate,
      SystemPort.Parity,
      SystemPort.DataBits,
      SystemPort.StopBits
    );

    SystemPort.Close();
    IsOpen = SystemPort.IsOpen;
    SystemPort = unopenedCopy;
  }

  private void ProcessReceivedData(object sender, SerialDataReceivedEventArgs e)
  {
    var port = (SerialPort)sender;
    var buffer = new byte[port.BytesToRead];
    port.Read(buffer, 0, buffer.Length);
    AddDataReceived(buffer);
  }

  protected override void Listen()
  {
    if (SystemPort is null)
      throw new InvalidOperationException("Cannot listen on the hardware connection without first creating a port.");

    SystemPort.DataReceived += ProcessReceivedData;
  }

  protected override void StopListening()
  {
    if (SystemPort is null)
      throw new InvalidOperationException("Cannot stop listening on the hardware connection without first creating a port.");

    SystemPort.DataReceived -= ProcessReceivedData;
  }

  protected override void SendOutboundMessage(SerialCommand command)
  {
    if (!IsOpen || SystemPort is null)
      throw new InvalidOperationException("Cannot send message as the serial port is not open.");

    var serializedData = command.SerializedData;
    SystemPort.Write(serializedData, 0, serializedData.Length);
  }
}
