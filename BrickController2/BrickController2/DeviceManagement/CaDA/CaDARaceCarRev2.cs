using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;
using System.Buffers.Binary;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// CaDA RaceCar - hardware revision 2
/// </summary>
internal class CaDARaceCarRev2 : BluetoothAdvertisingDevice
{
    private readonly byte[] _payloadTemplate;
    private readonly ICaDAPlatformService _cadaPlatformService;
    private readonly OutputValuesGroup<short> _outputValues = new(3);

    public CaDARaceCarRev2(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService,
            ICaDADeviceManager cadaManager, ICaDAPlatformService cadaPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService)
    {
        _cadaPlatformService = cadaPlatformService;

        if (deviceData?.Length == 16)
        {
            _payloadTemplate = deviceData;
            // seed
            _payloadTemplate[3] = deviceData[5];
            _payloadTemplate[4] = deviceData[6];
            // app id from manager
            BinaryPrimitives.TryWriteUInt16LittleEndian(_payloadTemplate.AsSpan(5), cadaManager.AppId);
        }
        else
        {
            throw new ApplicationException($"Invalid {nameof(deviceData)} array!");
        }
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
        // compose payload
        var payload = _payloadTemplate.AsSpan();

        // fill values
        _outputValues.TryGetValues(out var values);

        byte throttle = (byte)Math.Max(0, Math.Min(0x80 - values[0], 0xFF)); // speed value - reversed
        byte steering = (byte)Math.Max(0, Math.Min(0x80 + values[1], 0xFF));
        byte lights = (byte)(values[2] > 0 ? 0x01 : 0x00);
        byte sequence = (getConnectTelegram || (values[0] == 0.0f && values[1] == 0.0f))
            ? (byte)0xA1
            : (byte)(payload[11] + 1);

        // header: PAIRING : COMMAND
        payload[0] = getConnectTelegram ? (byte)0xAA : (byte)0xBB;
        payload[15] = getConnectTelegram ? (byte)0xA0 : (byte)0xB0;

        // Calculate Offset / Checksum (Byte 10)
        // Formula: Offset = (AppId_1 + AppId_2 + Byte_0 + Byte_15 + Byte_9 + Byte_11 + 0x11 - Steering - Throttle) mod 256
        int offsetSum = payload[5] + payload[6] + payload[0] + payload[15] + lights + sequence + 0x11
                + 512 // negative modulo math safely
                - steering - throttle;

        // Casting to byte automatically handles the modulo 256 wrap-around
        byte offset = (byte)offsetSum;

        // 5. Encode the Joystick Axes (Bytes 7 & 8) - zero is 0x80
        payload[7] = (byte)(throttle + offset);
        payload[8] = (byte)(steering + offset);
        payload[9] = lights;
        payload[10] = offset;
        payload[11] = sequence;

        return _cadaPlatformService.TryGetRfPayload(ManufacturerId, payload, out currentData);
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
        => new(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
}
