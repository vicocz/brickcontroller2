using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// CaDA RaceCar
/// </summary>
internal class CaDARaceCar : BluetoothAdvertisingDevice
{
    /// <summary>
    /// byte-array including DeviceAddress, AppID and channelData
    /// </summary>
    private readonly byte[] _controlDataArray = new byte[] // 16
    {
            0x75, //  [0] const 0x75 (117)
            0x13, //  [1] 0x13 (19) STATUS_CONTROL
            0x00, //  [2] DeviceAddress
            0x00, //  [3] DeviceAddress
            0x00, //  [4] DeviceAddress
            0x00, //  [5] AppID
            0x00, //  [6] AppID
            0x00, //  [7] AppID
            0x00, //  [8] ChannelData random
            0x00, //  [9] ChannelData random
            0x80, // [10] ChannelData verticalValue (min= 0x80 (128))
            0x80, // [11] ChannelData horizontalValue (min= 0x80 (128))
            0x00, // [12] ChannelData lightValue
            0x00, // [13] ChannelData 
            0x00, // [14] ChannelData 
            0x00, // [15] ChannelData 
    };

    private readonly ICaDAPlatformService _cadaPlatformService;
    private readonly int[] _outputValues = new int[3];
    private readonly Random _rnd = new Random();

    public CaDARaceCar(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ICaDAPlatformService cadaPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService)
    {
        _cadaPlatformService = cadaPlatformService;

        if (deviceData?.Length == 18)
        {
            // DeviceData-Array is the manufacturer specific data inside the response telegram sent when
            // * scanning for the device.
            // * loading the device from database

            // It's containing:
            // * DeviceAddress of the real CaDA device
            // * AppID sent from this App on scanning
            // These values are patched into the DataArray wich is advertised to control the device.
            _controlDataArray[2] = deviceData[4]; // DeviceAddress
            _controlDataArray[3] = deviceData[5]; // DeviceAddress
            _controlDataArray[4] = deviceData[6]; // DeviceAddress

            _controlDataArray[5] = deviceData[7]; // AppID
            _controlDataArray[6] = deviceData[8]; // AppID
            _controlDataArray[7] = deviceData[9]; // AppID
        }
        else
        {
            throw new ApplicationException("Invalid devicedata array!");
        }
    }
    public override DeviceType DeviceType => DeviceType.CaDA_RaceCar;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => CaDAProtocol.ManufacturerID;

    public override int NumberOfChannels => 3;

    public override void SetOutput(int channelNo, float value)
    {
        CheckChannel(channelNo);
        value = CutOutputValue(value);

        var intValue = (int)(value * 0x7F); // scale and cast

        lock (_outputLock)
        {
            if (_outputValues[channelNo] != intValue)
            {
                _outputValues[channelNo] = intValue;

                // notify data changed
                _bluetoothAdvertisingDeviceHandler.NotifyDataChanged(false);
            }
        }
    }
    protected override void InitDevice()
    {
    }

    protected bool TryGetTelegram(bool getConnectTelegram, out byte[] currentData)
    {
        ushort random = (ushort)_rnd.Next(ushort.MinValue, ushort.MaxValue);

        byte[] channelDataArray;

        lock (_outputLock)
        {
            channelDataArray = new byte[]// 8
            {
                (byte)(random & 0xFF),
                (byte)((random >> 8) & 0xFF),
                (byte)Math.Max(0, Math.Min(0x80 - _outputValues[0], 0xFF)), // speed value - reversed
                (byte)Math.Max(0, Math.Min(0x80 + _outputValues[1], 0xFF)), // 
                (byte)Math.Max(0, Math.Min(0x80 + _outputValues[2], 0xFF)), // light on/off
                0,
                0,
                0
            };
        }

        CaDAProtocol.Encrypt(channelDataArray);

        Array.Copy(channelDataArray, 0, _controlDataArray, 8, 8);

        _controlDataArray[0] = 0x75; // 0x75 (117)
        _controlDataArray[1] = 0x13; // 0x13 (19);

        return _cadaPlatformService.TryGetRfPayload(_controlDataArray, out currentData);
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
    }
}
