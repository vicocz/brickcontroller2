using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using BrickController2.UI.Services.Preferences;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Manager for CaDA devices
/// </summary>
public class CaDADeviceManager : BluetoothDeviceManagerBase, IBluetoothLEAdvertiserDeviceScanInfo, IBluetoothLEDeviceManager
{
    private const string SECTION = "CaDA";
    private const string APPIDKEY = "AppID";

    private readonly ICaDAPlatformService _cadaPlatformService;

    // this identifier is patched into the advertising data to identify the app
    private readonly byte[] _appIdChecksumMaskArray;

    public CaDADeviceManager(IPreferencesService preferencesService, ICaDAPlatformService cadaPlatformService)
    {
        _cadaPlatformService = cadaPlatformService;
        _appIdChecksumMaskArray = CaDADeviceManager.GetAppIdentifier(preferencesService);
    }

    public AdvertisingInterval AdvertisingIterval => AdvertisingInterval.Min;

    public TxPowerLevel TXPowerLevel => TxPowerLevel.Max;

    public ushort ManufacturerId => CaDAProtocol.ManufacturerID;

    /// <summary>
    /// Create an byte-array to be advertised on device-scan
    /// </summary>
    /// <returns>byte-array to be advertised on device-scan</returns>
    public byte[] CreateScanData()
    {
        byte[] pairingDataArray = new byte[] // 16
        {
              0x75, //  [0] const 0x75 (117)
              0x10, //  [1] 0x17 (23) STATUS_UNPAIRING - else - 0x10 (16)
              0x00, //  [2] DeviceAddress
              0x00, //  [3] DeviceAddress
              0x00, //  [4] DeviceAddress
              _appIdChecksumMaskArray[0], //  [5] AppID
              _appIdChecksumMaskArray[1], //  [6] AppID
              _appIdChecksumMaskArray[2], //  [7] AppID
              0x00, //  [8] 
              0x00, //  [9] 
              0x80, // [10] min 128
              0x80, // [11] min 128
              0x00, // [12] 
              0x00, // [13] 
              0x00, // [14] 
              0x00, // [15] 
        };

        _cadaPlatformService.TryGetRfPayload(pairingDataArray, out byte[] rf_payload_Array);

        return rf_payload_Array;
    }

    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult, FoundDevice template, ushort manufacturerId, ReadOnlySpan<byte> manufacturerData, out FoundDevice device)
    {
        switch (manufacturerId)
        {
            case 0xfff0:
                if (IsCadaRaceCar(manufacturerData))
                {
                    // the origin deviceAddress is changing on every scan-response
                    // but inside the manufacturerData are 3 bytes identifying the device
                    string deviceAddress = BitConverter.ToString(manufacturerData.Slice(4, 3).ToArray()).ToLower(); // change device address

                    device = template with
                    {
                        DeviceType = DeviceType.CaDA_RaceCar,
                        DeviceAddress = deviceAddress,        // change device address, 
                        DeviceName = $"CaDA {deviceAddress}"  // an empty devicename is given so create one
                    };
                    return true;
                }
                break;

                // extend if needed to other CaDA devices
        }
        // no match
        device = default;
        return false;
    }
    /// <summary>
    /// Check if manufacturerData is a scan-response from a CaDA RaceCar
    /// </summary>
    /// <param name="manufacturerData">byte-array with manufacturer data to check</param>
    /// <returns>true, if byte-array matches</returns>
    private bool IsCadaRaceCar(ReadOnlySpan<byte> manufacturerData)
    {
        return
            manufacturerData.Length == 18 &&
            manufacturerData[2] == 0x75 &&
            (manufacturerData[3] & 0x40) > 0 &&
            manufacturerData[7] == _appIdChecksumMaskArray[0] && // response has to have the same appId
            manufacturerData[8] == _appIdChecksumMaskArray[1] &&
            manufacturerData[9] == _appIdChecksumMaskArray[2];
    }

    /// <summary>
    /// gets or creates an App-persistant AppIdentifier
    /// </summary>
    /// <param name="preferencesService">reference to preferencesService singleton</param>
    /// <returns>byte array containing the AppIdentifier</returns>
    private static byte[] GetAppIdentifier(IPreferencesService preferencesService)
    {
        byte[] appIdChecksumMaskArray;
        // gets or creates an App-persistant AppIdentifier
        try
        {
            if (preferencesService.ContainsKey(APPIDKEY, SECTION))
            {
                // throws exception if converting went wrong
                appIdChecksumMaskArray = Convert.FromBase64String(preferencesService.Get(APPIDKEY, string.Empty, SECTION));

                // check minimum length
                if (appIdChecksumMaskArray?.Length >= 3)
                {
                    return appIdChecksumMaskArray; // valid
                }
            }
        }
        catch // catch all exceptions
        {
        }

        // create new byte[] with random values
        // * on first run
        // * on exception
        // * on length to short
        appIdChecksumMaskArray = new byte[3];

        Random.Shared.NextBytes(appIdChecksumMaskArray);

        try
        {
            preferencesService.Set(APPIDKEY, Convert.ToBase64String(appIdChecksumMaskArray), SECTION);
        }
        catch // catch all exceptions to keep app alive
        {
        }

        return appIdChecksumMaskArray;
    }
}