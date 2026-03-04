using System;
using BrickController2.PlatformServices.BluetoothLE;

using static BrickController2.DeviceManagement.Vengit.SBrickProtocol;

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
            var productId = GetProductId(manufacturerData);

            device = productId switch
            {
                PRODUCT_ID_SBRICK => new FoundDevice(scanResult, DeviceType.SBrick, manufacturerData),
                PRODUCT_ID_SBRICK_LIGHT => new FoundDevice(scanResult, DeviceType.SBrickLight, manufacturerData),

                _ => default
            };
            return device.DeviceType != DeviceType.Unknown;
        }

        device = default;
        return false;
    }

    private static byte GetProductId(ReadOnlySpan<byte> manufacturerData)
    {
        // walk through SBrick Data Records to look for 0x00 Product type
        int length = 2;
        ReadOnlySpan<byte> dataRecord = manufacturerData;

        while (length < dataRecord.Length)
        {
            dataRecord = dataRecord[length..];
            length = 1 + dataRecord[0];

            if (length > 2 &&
                dataRecord.Length >= 3 &&
                dataRecord[1] == DATA_RECORD_PRODUCT_TYPE)
            {
                return dataRecord[2];
            }
        }

        return PRODUCT_ID_UNKNOWN;
    }
}
