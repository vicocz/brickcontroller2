using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Manager for MouldKing devices
/// </summary>
public class MouldKingDeviceManager : BluetoothDeviceManagerBase
{
    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult,
        FoundDevice template,
        ushort manufacturerId,
        ReadOnlySpan<byte> manufacturerData,
        out FoundDevice device)
    {
        switch (manufacturerId)
        {
            case 0xac33:
                device = template with { DeviceType = DeviceType.MK_DIY };
                return true;
        }
        // no match
        device = default;
        return false;
    }
}
