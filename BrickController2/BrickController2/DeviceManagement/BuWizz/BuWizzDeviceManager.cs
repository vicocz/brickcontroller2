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
        ReadOnlySpan<byte> completeLocalName;
        switch (manufacturerId)
        {
            case 0x4d48:
                device = template with { DeviceType = DeviceType.BuWizz };
                return true;

            case 0x054e:
                if (scanResult.TryGetCompleteLocalName(out completeLocalName))
                {
                    if (completeLocalName.SequenceEqual("BuWizz"u8)) // BuWizz
                    {
                        device = template with { DeviceType = DeviceType.BuWizz2 };
                    }
                    else
                    {
                        device = template with { DeviceType = DeviceType.BuWizz3 };
                    }
                    return true;
                }
                break;

            case 0x4505: // BuWizz2 has new ID since firmware 1.2.30
                if (scanResult.TryGetCompleteLocalName(out completeLocalName))
                {
                    if (completeLocalName.SequenceEqual("BuWizz2"u8)) // BuWizz2
                    {
                        device = template with { DeviceType = DeviceType.BuWizz2 };
                        return true;
                    }
                }
                break;
        }
        // no match
        device = default;
        return false;
    }
}
