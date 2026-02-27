using System;

namespace Ares.Toolkit.Serial.Commands;

internal interface ISerialResponseParser
{
  bool TryParseResponse(SerialBlock[] buffer, out SerialResponse? response, out ArraySegment<byte>? dataToRemove);
}
