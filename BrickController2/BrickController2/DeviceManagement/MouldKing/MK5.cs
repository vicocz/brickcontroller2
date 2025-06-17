using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK 5.0 Module
/// </summary>
internal class MK5 : MKBaseNibble, IDeviceType<MK5>
{
    public const string Device = "Device";

    /// <summary>
    /// Telegram connect to MK5.0
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
      : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, 0, Telegram_Connect, Telegram_Base)
    {
    }

    public override DeviceType DeviceType => Type;

    public static DeviceType Type => DeviceType.MK5;

    public static string TypeName => "MK 5.0";

    public override int NumberOfChannels => 4;

    /// <summary>
    /// ManufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    /// <inheritdoc/>>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler() =>
        // there is only one instance of MK5.0
        new(_bleService, ManufacturerId, TryGetTelegram, ReconnectTimeSpan);


    protected override Func<float, (byte, bool)> CreateSetChannel(int channelNo)
    {
        return channelNo switch
        {
            0 => (float value) => SetOutput_AnalogChannel(value),
            1 => (float value) => SetOutput_AnalogChannel(value),
            2 => (float value) => SetOutput_Shot(value),
            3 => (float value) => SetOutput_AnalogChannel(value),
            _ => throw new ArgumentException("Illegal Argument", nameof(channelNo))
        };
    }

    private (byte, bool) SetOutput_AnalogChannel(float value)
    {
        // MK5: ZeroValueNibble = 0x00, Range_pos_Offset = 0x08
        // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
        // value == 0:                0
        // value >  0:                  9 A B C D E F    range_pos: 0x07
        const byte ZeroValueNibble = 0x00;
        const byte Range_pos_Offset = 0x08;
        const int Range_pos = 0x07;
        const int Range_neg = 0x07;

        if (value < 0)
        {
            float value_abs = Math.Min(0x07, -value * Range_neg);
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            if (setValue_nibble == 0) // replace zero with ZeroValueNibble
            {
                return (ZeroValueNibble, true);
            }
            else
            {
                return (setValue_nibble, false);
            }
        }
        else if (value > 0)
        {
            float value_abs = Math.Min(0x0F, (value * Range_pos) + Range_pos_Offset);
            byte setValue_nibble = (byte)(0x0F & (byte)(value_abs));

            return (setValue_nibble, false);
        }
        else
        {
            return (ZeroValueNibble, true);
        }
    }

    private (byte, bool) SetOutput_Shot(float value)
    {
        // Tank fires a shot when value is set to 0x0F
        if (value < 0)
        {
            return (0x0F, false);
        }
        else if (value > 0)
        {
            return (0x0F, false);
        }
        else
        {
            return (0x00, true);
        }
    }
}
