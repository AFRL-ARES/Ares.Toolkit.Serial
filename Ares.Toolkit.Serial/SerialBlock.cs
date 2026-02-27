using System;

namespace Ares.Toolkit.Serial
{
  internal record SerialBlock(byte[] Data, DateTime Timestamp);
}
