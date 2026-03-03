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
    private readonly OutputValuesGroup<float> _outputValues = new(3);
    private readonly byte _sequenceInitialValue;

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
        _sequence = _sequenceInitialValue = deviceData[11];
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

        if (_outputValues.SetOutput(channelNo, value))
        {
            _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, value == 0.0f);
            _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
        }
    }

    protected override void InitDevice()
    {
        // init sequence
        _sequence = _sequenceInitialValue;
    }

    protected override void DisconnectDevice()
    {
    }

    protected internal bool TryGetTelegram(bool getConnectTelegram, out byte[] currentData)
    {
        // fill values
        _outputValues.TryGetValues(out var values);

        // Map input (-1.0 to 1.0) to Throttle (0xFF to 0x00)
        byte throttle = (byte)Math.Clamp(128f - (values[0] * 128f), 0, 0xFF);
        // Map input (-1.0 to 1.0) to Steering (0x00 to 0xFF)
        byte steering = (byte)Math.Clamp(128f + (values[1] * 128f), 0, 0xFF);
        // lights on/off
        byte lights = (byte)(Math.Abs(values[2]) > 0.5 ? 0x01 : 0x00);
        byte sequence = (getConnectTelegram || (throttle == 0x80 && steering == 0x80))
            ? _sequenceInitialValue
            : ++_sequence;

        // header: PAIRING : COMMAND
        var header = getConnectTelegram ? (byte)0xAA : (byte)0xBB;

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
            (byte)(_appId & 0xFF), (byte)((_appId >> 8) & 0xFF),
            // throttle, steering, lights
            throttle, steering, lights,
            // checksum, sequence
            0x00, sequence,
            // 4 bytes footer, ending with 0xA0 / 0xB0
            0xCC, 0xB8, 0x92, (byte)(header & 0xF0)
        ];

        // The checksum is the sum of all 16 UNENCRYPTED bytes mod 256.
        int checksum = 0;
        for (int i = 0; i < payload.Length; i++)
        {
            checksum += payload[i];
        }
        payload[10] = (byte)checksum;
        // Encrypt the Axes via Bitwise XOR (Bytes 7 & 8)
        payload[7] ^= payload[10];
        payload[8] ^= payload[10];

        return _cadaPlatformService.TryGetRfPayloadRev2(payload, out currentData);
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
        => new(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
}
