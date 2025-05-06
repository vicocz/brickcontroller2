using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Manager for SBrick devices
/// </summary>
public class SBrickDeviceManager : BluetoothDeviceManagerBase
{
    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult,
        FoundDevice template,
        ushort manufacturerId,
        ReadOnlySpan<byte> manufacturerData,
        out FoundDevice device)
    {
        if (manufacturerId == 0x0198)
        {
            device = template with { DeviceType = DeviceType.SBrick };
            return true;
        }

        // no, device not handled
        device = FoundDevice.Unknown;
        return false;
    }
}
