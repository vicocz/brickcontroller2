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
    private readonly byte[] _payloadTemplate;

    private readonly ICaDAPlatformService _cadaPlatformService;
    private readonly OutputValuesGroup<short> _outputValues = new(3);

    public CaDARaceCarRev2(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ICaDAPlatformService cadaPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService)
    {
        _cadaPlatformService = cadaPlatformService;

        if (deviceData?.Length == 16)
        {
            _payloadTemplate = deviceData;
            // seed
            _payloadTemplate[3] = deviceData[5];
            _payloadTemplate[4] = deviceData[6];
            //TODO app id
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

        var rawValue = (short)(value * 0x7F); // scale and cast

        if (_outputValues.SetOutput(channelNo, rawValue))
        {
            _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, rawValue == 0x80);
            _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
        }
    }

    //TODO just now, AppId should be resolved via manager
    internal void SetAppId(byte appId1, byte appId2)
    {
        _payloadTemplate[5] = appId1;
        _payloadTemplate[6] = appId2;
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

        // header: PAIRING : COMMAND
        payload[0] = getConnectTelegram ? (byte)0xAA : (byte)0xBB;
        payload[15] = getConnectTelegram ? (byte)0xA0 : (byte)0xB0;

        // fill values
        _outputValues.TryGetValues(out var values);

        // 3. Set Feature Flags (Byte 9)
        payload[9] = (byte)(values[2] > 0 ? 0x01 : 0x00);

        // 4. Calculate Offset / Checksum (Byte 10)
        // Formula: AppId_1 + AppId_2 + Byte_0 + Byte_15 + Byte_9 + Constant(0xB2)
        int offsetSum = payload[5] + payload[6] + payload[0] + payload[15] + payload[9] + 0xB2;

        // Casting to byte automatically handles the modulo 256 wrap-around
        byte offset = (byte)offsetSum;
        payload[10] = offset;

        // 5. Encode the Joystick Axes (Bytes 7 & 8) - zero is 0x80
        payload[7] = (byte)(0x80 + values[0] + offset);
        payload[8] = (byte)(0x80 + values[1] + offset);

        return _cadaPlatformService.TryGetRfPayload(ManufacturerId, payload, out currentData);
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
        => new(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
}
