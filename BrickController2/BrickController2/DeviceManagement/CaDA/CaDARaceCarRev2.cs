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
    // 7 bytes: AA 11 11 Seed1 Seed2 AppId1, AppId2
    private readonly byte[] _devicePrefix;
    private readonly byte[] _deviceHardwareId;


    private readonly ICaDAPlatformService _cadaPlatformService;
    private readonly OutputValuesGroup<short> _outputValues = new(3);

    public CaDARaceCarRev2(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ICaDAPlatformService cadaPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService)
    {
        _cadaPlatformService = cadaPlatformService;

        // revisions
        if (deviceData?.Length == 16)
        {
            _devicePrefix = deviceData.AsSpan(0, 7).ToArray();
            // seed
            _devicePrefix[3] = deviceData[5];
            _devicePrefix[4] = deviceData[6];
            //TODO app id
            _devicePrefix[5] = 0xAD;
            _devicePrefix[6] = 0x42;

            _deviceHardwareId = deviceData.AsSpan(11, 5).ToArray();
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
        _outputValues.SetOutput(channelNo, rawValue);
    }

    protected override void InitDevice()
    {
    }

    protected override void DisconnectDevice()
    {
    }

    protected internal bool TryGetTelegram(bool getConnectTelegram, out byte[] currentData)
    {
        currentData =
        [
            // header
            0xC0, 0x00,
            // other data
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        ];

        // device data: prefix, seed, appId
        _devicePrefix.AsSpan(0, 7).CopyTo(currentData.AsSpan(2));
        currentData[2] = getConnectTelegram ? (byte)0xAA : (byte)0xBB;

        // command data
        if (getConnectTelegram)
        {
            ReadOnlySpan<byte> command = [0x6B, 0x6B, 0x00];
            command.CopyTo(currentData.AsSpan(9));
        }
        else
        {
            _outputValues.TryGetValues(out var values);

            currentData[9] = (byte)Math.Max(0, Math.Min(0x80 - values[0], 0xFF));
            currentData[10] = (byte)Math.Max(0, Math.Min(0x80 + values[1], 0xFF));
            currentData[11] = (byte)Math.Max(0, Math.Min(0x80 + values[2], 0xFF));
        }
        //currentData[9] ^= 0xAD;
       // currentData[10] ^= 0x42;
        currentData[12] = (byte)(currentData[9] + currentData[10] + currentData[11]);

        // hardware id
        _deviceHardwareId.AsSpan().CopyTo(currentData.AsSpan(13));
        currentData[17] = getConnectTelegram ? (byte)0xA0 : (byte)0xB0;

        ReadOnlySpan<byte> session = [0xEF, 0xF2, 0xC5, 0x67, 0x8F, 0x9F, 0xF1, 0xF8];
        session.CopyTo(currentData.AsSpan(18));

        return true;
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
        => new(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
}
