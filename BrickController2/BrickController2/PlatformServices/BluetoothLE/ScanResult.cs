using System;
using System.Collections.Generic;

using static BrickController2.Protocols.BluetoothLowEnergy;
namespace BrickController2.PlatformServices.BluetoothLE
{
    public class ScanResult
    {
        public ScanResult(string? deviceName, string? deviceAddress, IReadOnlyDictionary<byte, byte[]> advertismentData)
        {
            DeviceName = deviceName ?? string.Empty;
            DeviceAddress = deviceAddress ?? string.Empty;
            AdvertismentData = advertismentData;
        }

        public string DeviceName { get; }
        public string DeviceAddress { get; }
        public IReadOnlyDictionary<byte, byte[]> AdvertismentData { get; }

        public bool TryGetData(byte type, out ReadOnlySpan<byte> data)
        {
            if (AdvertismentData.TryGetValue(type, out var value))
            {
                data = value;
                return true;
            }
            data = null;
            return false;
        }

        public bool TryGetCompleteLocalName(out ReadOnlySpan<byte> localName)
            => TryGetData(ADTYPE_LOCAL_NAME_COMPLETE, out localName);
 
        public bool TryGetManufacturerData(out ReadOnlySpan<byte> manufacturerData)
            => TryGetData(ADTYPE_MANUFACTURER_SPECIFIC, out manufacturerData);
    }
}
