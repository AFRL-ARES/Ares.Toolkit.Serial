using System.Text;

namespace Ares.Toolkit.Serial.Commands;

/// <summary>
/// A simple serial command that takes a raw byte array.
/// </summary>
public class SimpleSerialCommand : SerialCommand
{
  private readonly byte[] _data;

  public SimpleSerialCommand(byte[] data)
  {
    _data = data;
  }

  public SimpleSerialCommand(string data, Encoding? encoding = null)
  {
    _data = (encoding ?? Encoding.ASCII).GetBytes(data);
  }

  protected override byte[] Serialize() => _data;
}
