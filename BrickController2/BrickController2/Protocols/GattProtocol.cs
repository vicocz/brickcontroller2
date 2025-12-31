using System;
using System.Buffers.Binary;

namespace BrickController2.DeviceManagement.Vengit;

/// <summary>
/// Generic GATT protocol
/// </summary>
internal static class GattProtocol
{
    public static readonly Guid DeviceInformationServiceUuid = new("0000180a-0000-1000-8000-00805f9b34fb");
    public static readonly Guid FirmwareRevisionCharacteristicUuid = new("00002a26-0000-1000-8000-00805f9b34fb");
    public static readonly Guid HardwareRevisionCharacteristicUuid = new("00002a27-0000-1000-8000-00805f9b34fb");
}
