using Ares.Toolkit.Serial.Commands;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ares.Toolkit.Serial;

public static class SerialConnectionExtensions
{
  /// <summary>
  /// Sends a command that expects a single line response terminated by <paramref name="terminator"/>.
  /// </summary>
  public static async Task<string> SendLine(this IAresSerialConnection connection, string command, string terminator = "\r\n", Encoding? encoding = null, CancellationToken token = default)
  {
    var cmd = new SimpleLineCommand(command, terminator, encoding);
    var response = await connection.Send(cmd, token);
    return response.Line;
  }

  /// <summary>
  /// Sends a command that doesn't expect a response.
  /// </summary>
  public static Task SendRaw(this IAresSerialConnection connection, byte[] data)
  {
    return connection.Send(new SimpleSerialCommand(data));
  }

  /// <summary>
  /// Sends a command as a string that doesn't expect a response.
  /// </summary>
  public static Task SendString(this IAresSerialConnection connection, string data, Encoding? encoding = null)
  {
    return connection.Send(new SimpleSerialCommand(data, encoding));
  }
}
