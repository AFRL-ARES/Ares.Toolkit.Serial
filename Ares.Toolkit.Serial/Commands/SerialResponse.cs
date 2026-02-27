using System;

namespace Ares.Toolkit.Serial.Commands;

public abstract class SerialResponse
{
  public Guid RequestId { get; internal set; }
}
