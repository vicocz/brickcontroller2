using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.Lego;

/// <summary>
/// Manager for LEGO© devices
/// </summary>
public class LegoDeviceManager : BluetoothDeviceManagerBase
{
    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult,
        FoundDevice template, ushort manufacturerId,
        ReadOnlySpan<byte> manufacturerData,
        out FoundDevice device)
    {
        if (manufacturerId == 0x0397 && manufacturerData.Length >= 4)
        {
            device = template with
            {
                DeviceType = manufacturerData[3] switch
                {
                    0x20 => DeviceType.DuploTrainHub,
                    0x40 => DeviceType.Boost,
                    0x41 => DeviceType.PoweredUp,
                    0x80 => DeviceType.TechnicHub,
                    0x84 => DeviceType.TechnicMove,

                    _ => DeviceType.Unknown
                }
            };
            return device.DeviceType != DeviceType.Unknown;
        }
        // no match
        device = default;
        return false;
    }

    protected override bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
    {
        if (serviceGuid == Wedo2Device.SERVICE_UUID)
        {
            device = template with { DeviceType = DeviceType.WeDo2 };
            return true;
        }
        // no match
        device = default;
        return false;
    }
}
