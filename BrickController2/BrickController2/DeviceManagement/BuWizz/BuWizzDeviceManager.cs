using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.BuWizz;

/// <summary>
/// Manager for BuWizz devices
/// </summary>
public class BuWizzDeviceManager : BluetoothDeviceManagerBase
{
    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult,
        FoundDevice template, ushort manufacturerId,
        ReadOnlySpan<byte> manufacturerData,
        out FoundDevice device)
    {
        // Discovering BuWizz device is based on the following information:
        // 4E:05:’B’:’W’:’x’:’y’ where x and y are firmware version
        // 05:45:’B’:’W’:’x’:’y’ where x and y are firmware version

        device = manufacturerData switch
        {
            // legacy BuWizz 1 - manufacturer ID only
            [0x48, 0x4d, ..] => template with { DeviceType = DeviceType.BuWizz },
            // BuWizz 3 prefix - 
            [0x4e, 0x05, 0x42, 0x57, 0x03, ..] => template with { DeviceType = DeviceType.BuWizz3 },
            // BuWizz 2 with older firmware -
            [0x4e, 0x05, ..] or
            // BuWizz2 with new ID since firmware 1.2.30
            [0x05, 0x45, 0x42, 0x57, 0x02, ..] => template with { DeviceType = DeviceType.BuWizz2 },

            _ => default,
        };

        return device.DeviceType != DeviceType.Unknown;
    }
    
    protected override bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
    {
        // this is prefered way however UUID might not be available on some platforms
        device = serviceGuid switch
        {
            // BuWizz2 (firmware 1.2.30 +)
            { } when serviceGuid == BuWizz2Device.SERVICE_UUID => template with { DeviceType = DeviceType.BuWizz2 },
            // BuWizz3
            { } when serviceGuid == BuWizz3Device.SERVICE_UUID => template with { DeviceType = DeviceType.BuWizz3 },

            _ => default
        };

        return device.DeviceType != DeviceType.Unknown;
    }
}
