namespace Ares.Toolkit.Serial
{
  /// <summary>
  /// Standard serial protocol identifiers.
  ///
  /// Devices may also specify custom protocol identifiers.
  /// Devices using compatible protocols may share a serial bus.
  /// </summary>
  public static class SerialDeviceProtocols
  {
    /// <summary>
    /// Dedicated protocol, where the device has exclusive access to the serial bus.
    /// </summary>
    public const string Dedicated = "DEDICATED";
    public const string ModbusRtu = "MODBUS_RTU";
    public const string ModbusAscii = "MODBUS_ASCII";
    public const string ModbusTpu = "MODBUS_TPU";
  }
}