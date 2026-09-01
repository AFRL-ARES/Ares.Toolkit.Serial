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

    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
    public string EndOfInput { get; set; } = string.Empty;
    public string Protocol { get; set; } = SerialDeviceProtocols.Dedicated;
}
