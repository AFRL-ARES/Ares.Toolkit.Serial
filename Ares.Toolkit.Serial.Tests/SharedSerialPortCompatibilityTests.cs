using System;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.Serialization;
using Ares.Toolkit.Serial;
using NUnit.Framework;

namespace Ares.Toolkit.Serial.Tests;

[TestFixture]
public class SharedSerialPortCompatibilityTests
{
  private static SharedSerialPort CreateSharedPort(
    SerialPortConnectionInfo baseInfo,
    string portName = "COM_TEST",
    int existingConnections = 0)
  {
    var type = typeof(SharedSerialPort);
    var instance = (SharedSerialPort)FormatterServices.GetUninitializedObject(type);

    // Initialize lock and connection set
    var stateLockField = type.GetField("_stateLock",
      BindingFlags.NonPublic | BindingFlags.Instance);
    stateLockField!.SetValue(instance, new object());

    var connectionsField = type.GetField("_connections",
      BindingFlags.NonPublic | BindingFlags.Instance);
    var connections = new HashSet<AresHardwareConnection>();
    for(var i = 0; i < existingConnections; i++)
    {
      var placeholder = (AresHardwareConnection)FormatterServices.GetUninitializedObject(typeof(AresHardwareConnection));
      connections.Add(placeholder);
    }
    connectionsField!.SetValue(instance, connections);

    // Initialize ConnectionInfo and PortName
    var connectionInfoField = type.GetField("<ConnectionInfo>k__BackingField",
      BindingFlags.NonPublic | BindingFlags.Instance);
    connectionInfoField!.SetValue(instance, baseInfo);

    var portNameField = type.GetField("<PortName>k__BackingField",
      BindingFlags.NonPublic | BindingFlags.Instance);
    portNameField!.SetValue(instance, portName);

    // Ensure disposed flag is false
    var disposedField = type.GetField("_disposed",
      BindingFlags.NonPublic | BindingFlags.Instance);
    disposedField!.SetValue(instance, false);

    return instance;
  }

  private static void Validate(SharedSerialPort sharedPort, SerialPortConnectionInfo requested)
  {
    var type = typeof(SharedSerialPort);
    var method = type.GetMethod("ValidateCompatibility",
      BindingFlags.NonPublic | BindingFlags.Instance);

    try
    {
      method!.Invoke(sharedPort, new object[] { requested });
    }
    catch (TargetInvocationException outerException) when (outerException.InnerException is not null)
    {
      // Re-throw the inner exception so Assert.Throws sees the real type (InvalidOperationException)
      throw outerException.InnerException!;
    }
  }

  [Test]
  public void DedicatedProtocol_CannotBeShared()
  {
    var baseInfo = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.None,
      dataBits: 8,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.Dedicated);

    var sharedPort = CreateSharedPort(baseInfo, portName: "COM1", existingConnections: 1);

    var requested = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.None,
      dataBits: 8,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.Dedicated);

    var ex = Assert.Throws<InvalidOperationException>(() => Validate(sharedPort, requested));
    Assert.That(ex!.Message, Does.Contain("configured as a dedicated connection"));
  }

  [Test]
  public void DifferentProtocols_CannotShareBus()
  {
    var baseInfo = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.None,
      dataBits: 8,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.ModbusRtu);

    var sharedPort = CreateSharedPort(baseInfo, portName: "COM2", existingConnections: 1);

    var requested = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.None,
      dataBits: 8,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.ModbusAscii);

    var ex = Assert.Throws<InvalidOperationException>(() => Validate(sharedPort, requested));
    Assert.That(ex!.Message, Does.Contain("already using protocol"));
    Assert.That(ex.Message, Does.Contain(SerialDeviceProtocols.ModbusRtu));
    Assert.That(ex.Message, Does.Contain(SerialDeviceProtocols.ModbusAscii));
  }

  [Test]
  public void DifferentBaudRates_CannotShareBus()
  {
    var baseInfo = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.None,
      dataBits: 8,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.ModbusRtu);

    var sharedPort = CreateSharedPort(baseInfo, portName: "COM3", existingConnections: 1);

    var requested = new SerialPortConnectionInfo(
      baudRate: 19200,
      parity: Parity.None,
      dataBits: 8,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.ModbusRtu);

    var ex = Assert.Throws<InvalidOperationException>(() => Validate(sharedPort, requested));
    Assert.That(ex!.Message, Does.Contain("already configured for"));
    Assert.That(ex.Message, Does.Contain("9600"));
    Assert.That(ex.Message, Does.Contain("19200"));
  }

  [Test]
  public void CompatibleDevices_CanShareBus()
  {
    var baseInfo = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.Even,
      dataBits: 7,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.ModbusRtu);

    var sharedPort = CreateSharedPort(baseInfo, portName: "COM4", existingConnections: 1);

    var requested = new SerialPortConnectionInfo(
      baudRate: 9600,
      parity: Parity.Even,
      dataBits: 7,
      stopBits: StopBits.One,
      protocol: SerialDeviceProtocols.ModbusRtu);

    Assert.DoesNotThrow(() => Validate(sharedPort, requested));
  }
}
