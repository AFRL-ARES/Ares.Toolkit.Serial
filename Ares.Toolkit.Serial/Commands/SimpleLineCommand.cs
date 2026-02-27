using System.Text;

namespace Ares.Toolkit.Serial.Commands;

public class SimpleLineCommand : SerialCommandWithResponse<LineResponse>
{
  private readonly string _command;
  private readonly Encoding _encoding;

  public SimpleLineCommand(string command, string terminator = "\r\n", Encoding? encoding = null) 
    : base(new LineResponseParser(terminator, encoding))
  {
    _command = command + terminator;
    _encoding = encoding ?? Encoding.ASCII;
  }

  protected override byte[] Serialize() => _encoding.GetBytes(_command);
}
