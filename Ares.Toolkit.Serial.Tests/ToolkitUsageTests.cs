using System.IO.Ports;
using Ares.Toolkit.Serial.Commands;
using Ares.Toolkit.Serial.Simulation;
using NUnit.Framework;

namespace Ares.Toolkit.Serial.Tests;

[TestFixture]
public class ToolkitUsageTests
{
    private class MockLineDevice : AresSerialSimConnection
    {
        public MockLineDevice() : base(new SerialPortConnectionInfo(9600, Parity.None, 8, StopBits.One), "MockDevice")
        {
        }

        public override void SendInternally(byte[] bytes)
        {
            var command = System.Text.Encoding.ASCII.GetString(bytes);
            if (command.StartsWith("PING\r\n"))
            {
                AddDataReceived(System.Text.Encoding.ASCII.GetBytes("PONG\r\n"));
            }
            else if (command.StartsWith("HELLO\r\n"))
            {
                AddDataReceived(System.Text.Encoding.ASCII.GetBytes("WORLD\r\n"));
            }
        }
    }

    [Test]
    public async Task SendLine_ReturnsCorrectResponse()
    {
        await using var connection = new MockLineDevice();
        connection.AttemptOpen();

        var response = await connection.SendLine("PING");
        
        Assert.That(response, Is.EqualTo("PONG"));
    }

    [Test]
    public async Task Multiple_SendLine_Calls_Work()
    {
        await using var connection = new MockLineDevice();
        connection.AttemptOpen();

        var response1 = await connection.SendLine("PING");
        var response2 = await connection.SendLine("HELLO");
        
        Assert.That(response1, Is.EqualTo("PONG"));
        Assert.That(response2, Is.EqualTo("WORLD"));
    }

    [Test]
    public void Builder_Creates_Connection()
    {
        var builder = new SerialConnectionBuilder()
            .WithPort("COM3")
            .WithBaudRate(115200)
            .WithTimeout(TimeSpan.FromSeconds(5));
        
        var connection = builder.Build();
        
        Assert.That(connection, Is.Not.Null);
        Assert.That(connection.Name, Is.EqualTo("COM3"));
    }
}
