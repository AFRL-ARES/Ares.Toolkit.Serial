using System;

namespace Ares.Toolkit.Serial.Commands;

public delegate bool TryParseResponseDelegate<T>(byte[] buffer, out T? response, out ArraySegment<byte>? dataToRemove) where T : SerialResponse;

public class DelegateResponseParser<T> : SerialResponseParser<T> where T : SerialResponse
{
  private readonly TryParseResponseDelegate<T> _parser;

  public DelegateResponseParser(TryParseResponseDelegate<T> parser)
  {
    _parser = parser;
  }

  public override bool TryParseResponse(byte[] buffer, out T? response, out ArraySegment<byte>? dataToRemove)
  {
    return _parser(buffer, out response, out dataToRemove);
  }
}
