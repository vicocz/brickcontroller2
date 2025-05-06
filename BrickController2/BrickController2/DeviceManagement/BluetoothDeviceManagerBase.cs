using System;
using System.Collections.Generic;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using static BrickController2.Protocols.BluetoothLowEnergy;

namespace BrickController2.DeviceManagement;

public abstract class BluetoothDeviceManagerBase : IBluetoothLEDeviceManager
{
    public bool TryGetDevice(ScanResult scanResult, out FoundDevice device)
    {
        var advertismentData = scanResult.AdvertismentData;
        if (advertismentData == null)
        {
            device = FoundDevice.Unknown;
            return false;
        }
        // build device template using available data from scan
        var template = new FoundDevice(DeviceType.Unknown, scanResult.DeviceName, scanResult.DeviceAddress);

        // if there is no manufacturer data, try other methods
        if (!scanResult.TryGetData(ADTYPE_MANUFACTURER_SPECIFIC, out var manufacturerData) || manufacturerData.Length < 2)
        {
            // by exact service UUID present in advertisment data
            if (TryGetDeviceInfoByService(template, advertismentData, out device))
            {
                return true;
            }

            // by well known local name
            if (scanResult.TryGetLocalName(out var localName))
            {
                return TryGetDeviceByName(template, localName, out device);
            }

            return false;
        }
        // adjust device template
        template = template with
        {
            ManufacturerData = manufacturerData.ToArray()
        };
        var manufacturerId = manufacturerData.GetUInt16();
        return TryGetDeviceByManufacturerData(scanResult, template, manufacturerId, manufacturerData, out device);
    }

    protected virtual bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
    {
        device = FoundDevice.Unknown;
        return false;
    }

    protected virtual bool TryGetDeviceByManufacturerData(ScanResult scanResult, FoundDevice template, ushort manufacturerId, ReadOnlySpan<byte> manufacturerData, out FoundDevice device)
    {
        device = FoundDevice.Unknown;
        return false;
    }

    protected virtual bool TryGetDeviceByName(FoundDevice template, ReadOnlySpan<byte> localName, out FoundDevice device)
    {
        device = FoundDevice.Unknown;
        return false;
    }

    private bool TryGetDeviceInfoByService(FoundDevice template, IReadOnlyDictionary<byte, byte[]> advertismentData, out FoundDevice device)
    {
        // 0x06: 128 bits Service UUID type
        if (advertismentData.TryGetValue(ADTYPE_SERVICE_128BIT, out byte[]? serviceData) && serviceData != null && serviceData.Length == 16)
        {
            var serviceGuid = BluetoothLowEnergy.GetGuid(serviceData.AsSpan());
            return TryGetDeviceByServiceUiid(template, serviceGuid, out device);
        }
        // detect other types of UUID if needed

        device = FoundDevice.Unknown;
        return false;
    }
}