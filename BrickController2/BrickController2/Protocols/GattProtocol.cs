using System;

namespace BrickController2.Protocols;

/// <summary>
/// Generic GATT protocol
/// </summary>
internal static class GattProtocol
{
    public static class Services
    {
        /// <summary>
        /// Device Information Service UUID
        /// </summary>
        public static readonly Guid DeviceInformation = new("0000180a-0000-1000-8000-00805f9b34fb");
    }
    public static class Characteristics
    {
        /// <summary>
        /// Device Information Service: Firmware Revision Characteristic UUID
        /// </summary>
        public static readonly Guid FirmwareRevision = new("00002a26-0000-1000-8000-00805f9b34fb");
        /// <summary>
        /// Device Information Service: Hardware Revision Characteristic UUID
        /// </summary>
        public static readonly Guid HardwareRevision = new("00002a27-0000-1000-8000-00805f9b34fb");
    }
}
