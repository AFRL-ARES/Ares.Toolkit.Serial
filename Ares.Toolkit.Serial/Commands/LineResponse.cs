namespace Ares.Toolkit.Serial.Commands;

public class LineResponse : SerialResponse
{
  public LineResponse(string line)
  {
    Line = line;
  }

  public string Line { get; }

  public override string ToString() => Line;
}
