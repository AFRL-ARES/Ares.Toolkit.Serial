using System.IO.Ports;

namespace Ares.Toolkit.Serial;

public class SerialPortConnectionInfo
{
  public SerialPortConnectionInfo(int baudRate, Parity parity, int dataBits, StopBits stopBits)
  {
    BaudRate = baudRate;
    Parity = parity;
    DataBits = dataBits;
    StopBits = stopBits;
  }

  public SerialPortConnectionInfo(int baudRate, Parity parity, int dataBits, StopBits stopBits, string protocol)
  {
    BaudRate = baudRate;
    Parity = parity;
    DataBits = dataBits;
    StopBits = stopBits;
    Protocol = protocol;
  }

  public SerialPortConnectionInfo(int baudRate, Parity parity, int dataBits, StopBits stopBits, string protocol, string endOfInput)
  {
    BaudRate = baudRate;
    Parity = parity;
    DataBits = dataBits;
    StopBits = stopBits;
    Protocol = protocol;
    EndOfInput = endOfInput;
  }

  public int BaudRate { get; }
  public Parity Parity { get; }
  public int DataBits { get; }
  public StopBits StopBits { get; }
  public string EndOfInput { get; } = string.Empty;
  public string Protocol { get; } = SerialDeviceProtocols.Dedicated;
}
