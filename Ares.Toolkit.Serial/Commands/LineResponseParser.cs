using System;
using System.Text;

namespace Ares.Toolkit.Serial.Commands;

public class LineResponseParser : SerialResponseParser<LineResponse>
{
  private readonly string _terminator;
  private readonly Encoding _encoding;

  public LineResponseParser(string terminator = "\r\n", Encoding? encoding = null)
  {
    _terminator = terminator;
    _encoding = encoding ?? Encoding.ASCII;
  }

  public override bool TryParseResponse(byte[] buffer, out LineResponse? response, out ArraySegment<byte>? dataToRemove)
  {
    var content = _encoding.GetString(buffer);
    var index = content.IndexOf(_terminator, StringComparison.Ordinal);
    if(index < 0)
    {
      response = null;
      dataToRemove = null;
      return false;
    }

    var line = content.Substring(0, index);
    response = new LineResponse(line);
    var bytesToRemove = _encoding.GetByteCount(content.Substring(0, index + _terminator.Length));
    dataToRemove = new ArraySegment<byte>(buffer, 0, bytesToRemove);
    return true;
  }
}
