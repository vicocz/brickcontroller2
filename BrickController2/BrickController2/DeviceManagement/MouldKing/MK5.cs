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
    private static readonly byte[] Telegram_Connect = [0xad, 0x7b, 0xa7, 0x80, 0x80, 0x80, 0x4f, 0x52];

    /// <summary>
    /// Base Telegram
    /// </summary>
    private static readonly byte[] Telegram_Base = [0x7d, 0x7b, 0xa7, 0x00, 0x00, 0x80, 0x80, 0x80, 0x80, 0x82];

    /// <summary>
    /// After this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    public MK5(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, CHANNEL_START_OFFSET, Telegram_Connect, Telegram_Base)
    {
    }

    public override DeviceType DeviceType => DeviceType.MK5;

    public override int NumberOfChannels => 4;

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

    // MK5: ZeroValueNibble = 0x00, ZeroValueOffset = 0x08
    // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
    // value == 0:                0
    // value >  0:                  9 A B C D E F    range_pos: 0x07

    /// <summary>
    /// Gets the nibble value that represents zero in the current encoding scheme.
    /// </summary>
    protected override byte ZeroValueNibble => 0x00;

    /// <summary>
    /// Gets the offset for positive values
    /// </summary>
    protected override byte Range_pos_Offset => 0x08;

    /// <summary>
    /// Gets the range for positive values
    /// </summary>
    protected override int Range_neg => 0x07;

    /// <summary>
    /// Gets the range for negative values
    /// </summary>
    protected override int Range_pos => 0x07;

    /// <inheritdoc/>>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler() =>
        // MK5.0 needs a BluetoothAdvertiser per module
        new(_bleService, ManufacturerId, TryGetTelegram, ReconnectTimeSpan);
}
