using System;

namespace Ares.Toolkit.Serial.Commands;

internal interface ISerialCommandWithResponse
{
  Guid Id { get; internal set; }
  ISerialResponseParser ResponseParser { get; }
}
