using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK 4.0 Module
/// </summary>
internal class MK3_8 : MKBaseNibble, IDeviceType<MK3_8>
{
    public const string Device = "Device";

    /// <summary>
    /// Telegram to connect to MK3.8
    /// </summary>
    private static readonly byte[] Telegram_Connect = [ 0xB1, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x4F, 0xC1 ];

    /// <summary>
    /// Base Telegram for MK3.8
    /// * channels 0..3 start at offset 3 and are analog channels
    /// * channel 4 starts at offset 2 is left/right channel
    /// </summary>
    private static readonly byte[] Telegram_Base = [ 0x81, 0x7B, 0x00, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0xC2 ];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    public MK3_8(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, 0, Telegram_Connect, Telegram_Base)
    {
    }

    public override DeviceType DeviceType => Type;

    public static DeviceType Type => DeviceType.MK3_8;

    public static string TypeName => "MK 3.8";

    /// <summary>
    /// channel 0..3 are analog, 
    /// channel 4 is left/right channel
    /// </summary>
    public override int NumberOfChannels => 5;

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        // there is only one MK3.8 module
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, MK3_8.ReconnectTimeSpan);
    }

    /// <summary>
    /// Processes the value for a specified channel and returns the processed result along with a status flag.
    /// </summary>
    /// <remarks>Each channel number corresponds to a specific operation: <list type="bullet">
    /// <item><description>Channel 0: Tracks A + B forward or backward.</description></item> <item><description>Channel
    /// 1: Turret rotation.</description></item> <item><description>Channel 2: Canon shot.</description></item>
    /// <item><description>Channel 3: Turn on the spot.</description></item> <item><description>Channel 4: Virtual
    /// channel for locking the turret.</description></item> </list></remarks>
    /// <param name="channelNo">The channel number to process. Valid values are 0 through 4.</param>
    /// <param name="value">The input value to be processed for the specified channel.</param>
    /// <returns>A tuple containing the processed byte value and a boolean flag indicating the success or status of the
    /// operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not within the valid range of 0 through 4.</exception>
    protected override (byte value, bool flag) ProcessChannelValue(int channelNo, float value) => channelNo switch
    {
        0 => ProcessChannelValue_AnalogChannel(value),
        1 => ProcessChannelValue_AnalogChannel(value),
        2 => ProcessChannelValue_AnalogChannel(value),
        3 => ProcessChannelValue_AnalogChannel(value),
        4 => ProcessChannelValue_LeftRigthChannel(value),
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };

    /// <summary>
    /// Calculates the byte offset for the specified channel number.
    /// </summary>
    /// <param name="channelNo">The channel number for which to calculate the byte offset. Must be between 0 and 4, inclusive.</param>
    /// <returns>The byte offset corresponding to the specified channel number.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not between 0 and 4.</exception>
    protected override (int byteOffset, bool isLowerNibble) GetTargetPosition(int channelNo) => channelNo switch
    {
        4 => (2, true), // Channel 4 starts at offset 2
        _ => base.GetTargetPosition(channelNo)
    };

    /// <summary>
    /// Converts a floating-point value into a nibble representation for an analog channel output.
    /// </summary>
    /// <remarks>This method maps the input <paramref name="value"/> to a nibble (4-bit) representation based
    /// on its sign and magnitude. Negative values are scaled using a predefined negative range, positive values are
    /// scaled using a positive range with an offset,  and zero values are represented by a specific nibble
    /// value.</remarks>
    /// <param name="value">The floating-point value to be converted. Negative values, positive values, and zero are handled differently.</param>
    /// <returns>A tuple containing: <list type="bullet"> <item> <description><c>setValue_nibble</c>: The 4-bit nibble
    /// representation of the input value.</description> </item> <item> <description><c>zeroSet</c>: A boolean
    /// indicating whether the input value was zero (<see langword="true"/>) or not (<see
    /// langword="false"/>).</description> </item> </list></returns>
    private static (byte setValue_Nibble, bool zeroSet) ProcessChannelValue_AnalogChannel(float value)
    {
        // MK5: ZEROVALUE_NIBBLE = 0x00, Range_pos_Offset = 0x08
        // value <  0:  7 6 5 4 3 2 1                    RANGE_NEG: 0x07
        // value == 0:                0
        // value >  0:                  9 A B C D E F    range_pos: 0x07

        // MK3.8: ZEROVALUE_NIBBLE = 0x09, Range_pos_Offset = 0x09
        // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
        // value == 0:                0 9
        // value >  0:                    A B C D E F    range_pos: 0x06

        const byte RANGE_POS_OFFSET = 0x09;
        const int RANGE_POS = 0x06;
        const int RANGE_NEG = 0x07;

        const float MIN_NEG_RANGE_THRESHOLD = -1f / RANGE_NEG;    // Minimum value for negative range
        const float MIN_POS_RANGE_THRESHOLD = 1f / RANGE_POS;     // Minimum value for positive range

        const byte ZEROVALUE_NIBBLE = 0x09;

        if (value <= MIN_NEG_RANGE_THRESHOLD)
        {
            float value_abs = Math.Min(0x07, -value * RANGE_NEG);
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else if (value >= MIN_POS_RANGE_THRESHOLD)
        {
            float value_abs = Math.Min(0x0F, (value * RANGE_POS) + RANGE_POS_OFFSET);
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else
        {
            return (ZEROVALUE_NIBBLE, true);
        }
    }

    /// <summary>
    /// Converts a floating-point value into a nibble representation for an analog channel output.
    /// </summary>
    /// <remarks>This method maps the input <paramref name="value"/> to a nibble (4-bit) representation based
    /// on its sign and magnitude. Negative values are scaled using a predefined negative range, positive values are
    /// scaled using a positive range with an offset,  and zero values are represented by a specific nibble
    /// value.</remarks>
    /// <param name="value">The floating-point value to be converted. Negative values, positive values, and zero are handled differently.</param>
    /// <returns>A tuple containing: <list type="bullet"> <item> <description><c>setValue_nibble</c>: The 4-bit nibble
    /// representation of the input value.</description> </item> <item> <description><c>zeroSet</c>: A boolean
    /// indicating whether the input value was zero (<see langword="true"/>) or not (<see
    /// langword="false"/>).</description> </item> </list></returns>
    private static (byte setValue_Nibble, bool zeroSet) ProcessChannelValue_LeftRigthChannel(float value)
    {
        // MK5: ZEROVALUE_NIBBLE = 0x00
        // value <  0:              1                    RANGE_NEG: 0x07
        // value == 0:                0
        // value >  0:                  2                range_pos: 0x07

        const float MIN_NEG_RANGE_THRESHOLD = -1f / 7;    // Minimum value for negative range
        const float MIN_POS_RANGE_THRESHOLD = 1f / 7;     // Minimum value for positive range

        const byte ZEROVALUE_NIBBLE = 0x00;

        if (value <= MIN_NEG_RANGE_THRESHOLD)
        {
            float value_abs = 1;
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else if (value >= MIN_POS_RANGE_THRESHOLD)
        {
            float value_abs = 2;
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else
        {
            return (ZEROVALUE_NIBBLE, true);
        }
    }
}