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
            _payloadTemplate[5] = 0xAD;
            _payloadTemplate[6] = 0x42;
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

        // device data: AA vs BB flag
        payload[2] = getConnectTelegram ? (byte)0xAA : (byte)0xBB;

        // command data
        if (getConnectTelegram)
        {
            ReadOnlySpan<byte> command = [0x6B, 0x6B, 0x00];
            command.CopyTo(payload.Slice(7));
        }
        else
        {
            _outputValues.TryGetValues(out var values);

            payload[7] = (byte)Math.Max(0, Math.Min(0x80 - values[0], 0xFF));
            payload[8] = (byte)Math.Max(0, Math.Min(0x80 + values[1], 0xFF));
            payload[9] = (byte)Math.Max(0, Math.Min(0x80 + values[2], 0xFF));
        }
        //TODO XOR using AppId
        //currentData[9] ^= 0xAD;
        // currentData[10] ^= 0x42;
        payload[10] = (byte)(payload[7] + payload[8] + payload[9]);

        // update flags
        payload[15] = getConnectTelegram ? (byte)0xA0 : (byte)0xB0;

        return _cadaPlatformService.TryGetRfPayload(ManufacturerId, payload, out currentData);
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
        => new(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
}
