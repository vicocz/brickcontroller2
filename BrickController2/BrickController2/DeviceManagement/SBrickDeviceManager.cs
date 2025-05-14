using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Manager for SBrick devices
/// </summary>
public class SBrickDeviceManager : IBluetoothLEDeviceManager
{
    private static readonly byte[] ManufacturerId = { 0x98, 0x01 };

    public bool TryGetDevice(ScanResult scanResult, out FoundDevice device)
    {
        // check if there are any data and it matches Vengit prefix 0x0198
        if (scanResult.TryGetManufacturerData(out var manufacturerData) && manufacturerData.StartsWith(ManufacturerId))
        {
            device = new FoundDevice(scanResult, DeviceType.SBrick, manufacturerData);
            return true;
        }

        device = default;
        return false;
    }
}
