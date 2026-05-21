using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using static BrickController2.Protocols.BluetoothLowEnergy;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Base class, which provides a common interface for all Bluetooth LE device managers,
/// especially if it supports several types of detection
/// </summary>
public abstract class BluetoothDeviceManagerBase : IBluetoothLEDeviceManager
{
    public bool TryGetDevice(ScanResult scanResult, out FoundDevice device)
    {
        var advertismentData = scanResult.AdvertismentData;
        if (advertismentData == null)
        {
            device = default;
            return false;
        }
        // build device template using available data from scan
        var template = new FoundDevice(DeviceType.Unknown, scanResult.DeviceName, scanResult.DeviceAddress);

        // adjust device template if there is manufacturer data present
        if (scanResult.TryGetManufacturerData(out var manufacturerData) && manufacturerData.Length > 0)
        {
            template = template with
            {
                ManufacturerData = manufacturerData.ToArray()
            };
        }

        // by exact service UUID present in advertisment data
        if (TryGetDeviceInfoByService(template, scanResult, out device))
        {
            return true;
        }

        // if there is manufacturer data, try to apply it
        if (manufacturerData.Length >= 2)
        {
            var manufacturerId = manufacturerData.GetUInt16();
            if (TryGetDeviceByManufacturerData(scanResult, template, manufacturerId, manufacturerData, out device))
            {
                return true;
            }
        }

        // by well known local name
        if (scanResult.TryGetCompleteLocalName(out var localName))
        {
            return TryGetDeviceByCompleteLocalName(template, localName, out device);
        }

        device = default;
        return false;
    }

    /// <summary>
    /// Try to get device by service UUID
    /// </summary>
    /// <param name="template">Template of device based on the current advertisment data</param>
    /// <param name="serviceGuid">Service UUID to match</param>
    /// <param name="device">Resulting device</param>
    /// <returns>true if device was matched by service UUID</returns>
    protected virtual bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
    {
        device = default;
        return false;
    }

    /// <summary>
    /// Try to get device by manufacturer data
    /// </summary>
    /// <param name="scanResult">Current scan result being processed</param>
    /// <param name="template">Template of device based on the current advertisment data</param>
    /// <param name="manufacturerId">Company unique ID</param>
    /// <param name="manufacturerData">Complete collection of manufacturer data</param>
    /// <param name="device">Resulting device</param>
    /// <returns>true if device was matched by manufacturer data</returns>
    protected virtual bool TryGetDeviceByManufacturerData(ScanResult scanResult, FoundDevice template, ushort manufacturerId, ReadOnlySpan<byte> manufacturerData, out FoundDevice device)
    {
        device = default;
        return false;
    }

    /// <summary>
    /// Try to get device by local name
    /// </summary>
    /// <param name="template">Template of device based on the current advertisment data</param>
    /// <param name="localName">Collection of bytes representing complete local name advertisment data</param>
    /// <param name="device">Resulting device</param>
    /// <returns>true if device was matched by name</returns>
    protected virtual bool TryGetDeviceByCompleteLocalName(FoundDevice template, ReadOnlySpan<byte> localName, out FoundDevice device)
    {
        device = default;
        return false;
    }

    private bool TryGetDeviceInfoByService(FoundDevice template, ScanResult scanResult, out FoundDevice device)
    {
        // 0x06: 128 bits Service UUID type
        if (scanResult.TryGetData(ADTYPE_INCOMPLETE_SERVICE_128BIT, out var incompleteServiceData))
        {
            if (TryGetDeviceByServiceUiid(template, scanResult, incompleteServiceData, out device))
            {
                return true;
            }
        }
        
        // 0x07: 128 bits Service UUID type
        if (scanResult.TryGetData(ADTYPE_COMPLETE_SERVICE_128BIT, out var completeServiceData))
        {
            if (TryGetDeviceByServiceUiid(template, scanResult, completeServiceData, out device))
            {
                return true;
            }
        }
        // detect other types of UUID if needed

        device = default;
        return false;
    }

    private bool TryGetDeviceByServiceUiid(FoundDevice template, ScanResult scanResult, ReadOnlySpan<byte> serviceData, out FoundDevice device)
    {
        // go through all service UUIDs if needed
        while (serviceData.Length >= 16)
        {
            var serviceGuid = BluetoothLowEnergy.GetGuid(serviceData[..16]);
            if (TryGetDeviceByServiceUiid(template, serviceGuid, out device))
            {
                return true;
            }

            serviceData = serviceData[16..];
        }

        device = default;
        return false;
    }
}

