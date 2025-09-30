using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK 6.0 Module
/// </summary>
internal class MK6 : MKBaseByte, IDeviceType<MK6>
{
    public const string Device1 = "Device1";
    public const string Device2 = "Device2";
    public const string Device3 = "Device3";

    /// <summary>
    /// Telegram connect to MK6.0 (switch MK6.0 to Bluetooth mode)
    /// </summary>
    private static readonly byte[] Telegram_Connect = new byte[] { 0x6D, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x92, };

    /// <summary>
    /// Base Telegram for Device 1
    /// </summary>
    private static readonly byte[] Telegram_Base_Device_1 = new byte[] { 0x61, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x9E };

    /// <summary>
    /// Base Telegram for Device 2
    /// </summary>
    private static readonly byte[] Telegram_Base_Device_2 = new byte[] { 0x62, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x9D };

    /// <summary>
    /// Base Telegram for Device 3
    /// </summary>
    private static readonly byte[] Telegram_Base_Device_3 = new byte[] { 0x63, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x9C };

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    /// <summary>
    /// number of bytes containing channel values in base telegram
    /// </summary>
    protected override int BaseTelegram_ChannelBytesCount => 6;

    /// <summary>
    /// offset to position of first channel in base telegram
    /// </summary>
    protected override int BaseTelegram_ChannelStartOffset => 3;

    public MK6(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService, 3, MK6.Telegram_Connect, MK6.GetTelegramBase(address), mkPlatformService)
    {
    }


    public static DeviceType Type => DeviceType.MK6;

    public static string TypeName => "MK 6.0";

    public override DeviceType DeviceType => Type;

    public override int NumberOfChannels => 6;

    /// <summary>
    /// Get reference to Base-Telegram for the given address
    /// </summary>
    /// <param name="address">address</param>
    /// <returns>reference to Base-Telegram</returns>
    private static byte[] GetTelegramBase(string address)
    {
        return address switch
        {
            MK6.Device1 => Telegram_Base_Device_1,
            MK6.Device2 => Telegram_Base_Device_2,
            MK6.Device3 => Telegram_Base_Device_3,
            _ => throw new ArgumentException("Illegal Argument", nameof(address))
        };
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        // MK6.0 needs a BluetoothAdvertiser per module
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, MK6.ReconnectTimeSpan);
    }
}
