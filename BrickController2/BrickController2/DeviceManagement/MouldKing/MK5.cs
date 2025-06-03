using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK 5.0 Module
/// </summary>
internal class MK5 : MKBaseNible
{
    public const string Device = "Device";

    public const int CHANNEL_START_OFFSET = 3;

    /// <summary>
    /// Telegram connect to MK5.0 (switch MK5.0 to Bluetooth mode)
    /// </summary>
    private static readonly byte[] Telegram_Connect = [0xad, 0x3f, 0x4d, 0x80, 0x80, 0x80, 0xb9, 0x52];

    /// <summary>
    /// Base Telegram
    /// </summary>
    private static readonly byte[] Telegram_Base = [0x7d, 0x3f, 0x4d, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x82];

    /// <summary>
    /// After this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    /// <summary>
    /// ManufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    /// <summary>
    /// Number of bytes containing channel values in base telegram
    /// </summary>
    protected override int BaseTelegram_ChannelBytesCount => 2;

    /// <summary>
    /// Offset to position of first channel in base telegram
    /// </summary>
    protected override int BaseTelegram_ChannelStartOffset => CHANNEL_START_OFFSET;

    public MK5(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService, CHANNEL_START_OFFSET, Telegram_Connect, Telegram_Base, mkPlatformService)
    {
    }

    public override DeviceType DeviceType => DeviceType.MK5;

    public override int NumberOfChannels => 4;

    /// <inheritdoc/>>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler() =>
        // MK5.0 needs a BluetoothAdvertiser per module
        new(_bleService, ManufacturerId, TryGetTelegram, ReconnectTimeSpan);
}
