using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.BuWizz;

/// <summary>
/// Manager for BuWizz devices
/// </summary>
public class BuWizzDeviceManager : BluetoothDeviceManagerBase
{
    //  05:4E:’B’:’W’:’x’:’y’ where x and y are firmware version
    private static readonly byte[] BuWizz3Prefix = [0x4e, 0x05, 0x42, 0x57, 0x03];
    private static readonly byte[] BuWizz2Prefix = [0x05, 0x45, 0x42, 0x57, 0x02];

    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult,
        FoundDevice template, ushort manufacturerId,
        ReadOnlySpan<byte> manufacturerData,
        out FoundDevice device)
    {
        switch (manufacturerId)
        {
            case 0x4d48:
                device = template with { DeviceType = DeviceType.BuWizz };
                return true;

            case 0x054e:
                if (manufacturerData.StartsWith(BuWizz3Prefix))
                {
                    device = template with { DeviceType = DeviceType.BuWizz3 };
                }
                else
                {
                    device = template with { DeviceType = DeviceType.BuWizz2 };
                }
                return true;

            case 0x4505: // BuWizz2 has new ID since firmware 1.2.30
                if (manufacturerData.StartsWith(BuWizz2Prefix))
                {
                    device = template with { DeviceType = DeviceType.BuWizz2 };
                    return true;
                }
                break;
        }
        // no match
        device = default;
        return false;
    }
    
    protected override bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
    {
        // detect BuWizz2 (firmware 1.2.30+) and BuWizz3 by service UUID
        device = serviceGuid switch
        {
            {} when serviceGuid == BuWizz2Device.SERVICE_UUID => template with { DeviceType = DeviceType.BuWizz2 },
            {} when serviceGuid == BuWizz3Device.SERVICE_UUID => template with { DeviceType = DeviceType.BuWizz3 },
            _ => default
        };

        return device.DeviceType != DeviceType.Unknown;
    }
}
