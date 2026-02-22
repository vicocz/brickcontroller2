using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// CaDA RaceCar - hardware revision 2
/// </summary>
internal class CaDARaceCarRev2 : BluetoothAdvertisingDevice
{
    private const byte SEQUENCE_INITIAL_VALUE = 0xA1; // initial value for sequence byte in telegram

    private readonly byte[] _deviceId;
    private readonly ushort _appId;
    private readonly ICaDAPlatformService _cadaPlatformService;
    private readonly OutputValuesGroup<short> _outputValues = new(3);

    private byte _sequence;

    public CaDARaceCarRev2(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService,
            ICaDADeviceManager cadaManager, ICaDAPlatformService cadaPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService)
    {
        _cadaPlatformService = cadaPlatformService;

        if ((deviceData?.Length) != 16)
        {
            throw new ApplicationException($"Invalid {nameof(deviceData)} array!");
        }
        // persist device id (bytes 5 & 6) for later use in payload template
        _deviceId = [deviceData[5], deviceData[6]];
        // seed sequence with value from scan data
        _sequence = deviceData[11];
        _appId = cadaManager.AppId;
    }
    public override DeviceType DeviceType => DeviceType.CaDA_RaceCar_Rev2;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => CaDAProtocol.ManufacturerID;

    public override int NumberOfChannels => 3;

    public override void SetOutput(int channelNo, float value)
    {
        CheckChannel(channelNo);
        value = CutOutputValue(value);

        var rawValue = (short)(value * 0x60); // scale and cast

        if (_outputValues.SetOutput(channelNo, rawValue))
        {
            _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, rawValue == 0x80);
            _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
        }
    }

    protected override void InitDevice()
    {
    }

    protected override void DisconnectDevice()
    {
    }

    protected internal bool TryGetTelegram(bool getConnectTelegram, out byte[] currentData)
    {
        // fill values
        _outputValues.TryGetValues(out var values);

        byte throttle = (byte)Math.Max(0, Math.Min(0x80 - values[0], 0xFF)); // speed value - reversed
        byte steering = (byte)Math.Max(0, Math.Min(0x80 + values[1], 0xFF));
        byte lights = (byte)(values[2] > 0 ? 0x01 : 0x00);
        byte sequence = (getConnectTelegram || (values[0] == 0.0f && values[1] == 0.0f))
            ? SEQUENCE_INITIAL_VALUE
            : ++_sequence;

        // header: PAIRING : COMMAND
        var header = getConnectTelegram ? (byte)0xAA : (byte)0xBB;
        var footer = getConnectTelegram ? (byte)0xA0 : (byte)0xB0;
        var appId1 = (byte)(_appId & 0xFF);
        var appId2 = (byte)((_appId >> 8) & 0xFF);

        // Calculate Offset / Checksum (Byte 10)
        // Formula: Offset = (AppId_1 + AppId_2 + Byte_0 + Byte_15 + Byte_9 + Byte_11 + 0x11 - Steering - Throttle) mod 256
        int offsetSum = appId1 + appId2 + header + footer + lights + sequence + 0x11
                + 512 // negative modulo math safely
                - steering - throttle;

        // Casting to byte automatically handles the modulo 256 wrap-around
        byte offset = (byte)offsetSum;

        // compose payload of 16 bytes
        byte[] payload =
        [
            // manufacturerId
            header, 0x11,
            // CADA RaceCar?
            0x11,
            // DeviceId
            _deviceId[0], _deviceId[1],
            // 2 bytes AppID - zeros from the scan
            appId1, appId2,
            // throttle, steering, lights
             (byte)(throttle + offset), (byte)(steering + offset), lights,
            // offset, sequence - placeholders for now, will be calculated and filled later
            offset, sequence,
            // 4 bytes footer
            0xCC, 0xB8, 0x92, footer
        ];

        return _cadaPlatformService.TryGetRfPayload(ManufacturerId, payload, out currentData);
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
        => new(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
}
