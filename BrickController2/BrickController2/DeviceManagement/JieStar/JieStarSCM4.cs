using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Jie Star 4 Channel Smart Creative Module (SCM)
/// </summary>
internal class JieStarSCM4 : JieStarBase, IDeviceType<JieStarSCM4>
{
    public const string Device = "Device";

    /// <summary>
    /// Telegram to connect to the SCM4 device
    /// This telegram is sent on init and on reconnect conditions matching
    /// </summary>
    private static readonly byte[] Telegram_Connect = [0xa4, 0x1d, 0x74, 0x80, 0x80, 0x80, 0x80, 0x5b];

    /// <summary>
    /// Base Telegram for SCM4 device
    /// </summary>
    private static readonly byte[] Telegram_Base= [0x40, 0x1d, 0x74, 0x80, 0x80, 0x80, 0x80, 0xbf];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    public JieStarSCM4(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IJieStarPlatformService jieStarPlatformService, IJieStarDeviceManager jieStarDeviceManager)
      : base(name, address, deviceData, deviceRepository, bleService, jieStarPlatformService, jieStarDeviceManager, JieStarSCM4.Telegram_Connect, JieStarSCM4.Telegram_Base)
    {
    }

    public static DeviceType Type => DeviceType.JieStarSCM4;

    public static string TypeName => "JieStar SCM 4";

    public override DeviceType DeviceType => Type;

    /// <summary>
    /// Gets the number of channels supported by the device.
    /// <remarks><list type="bullet">
    /// <item><description>Channel 0..3: real existing channel</description></item> 
    /// </list></remarks>
    /// </summary>
    public override int NumberOfChannels => 4;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => JieStarProtocol.ManufacturerID;

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        // JieStarSCM4 needs a BluetoothAdvertiser per module
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, JieStarSCM4.ReconnectTimeSpan);
    }

    /// <summary>
    /// Processes the value for the specified analog channel and returns the processed result.
    /// </summary>
    /// <param name="channelNo">The channel number to process. Valid values are 0, 1, 2, 3.</param>
    /// <param name="value">The input value to be processed for the specified channel.</param>
    /// <returns>A tuple containing the processed value and a flag indicating the success of the operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not one of the valid channel numbers (0, 1, 2, 3).</exception>
    protected override (byte value, bool flag) ProcessChannelValue(int channelNo, float value) => channelNo switch
    {
        0 => SetOutput_AnalogChannel(value),
        1 => SetOutput_AnalogChannel(value),
        2 => SetOutput_AnalogChannel(value),
        3 => SetOutput_AnalogChannel(value),
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };

    /// <summary>
    /// Converts a floating-point value into a nibble representation for an analog channel output.
    /// </summary>
    /// <remarks>The method maps the input value to a nibble representation based on predefined ranges for
    /// positive, negative, and zero values. The zero value is represented by a specific nibble constant. The caller can
    /// use the returned boolean to determine if the nibble corresponds to the zero value.</remarks>
    /// <param name="value">The floating-point value to be converted. Negative values are mapped to the negative range, positive values are
    /// mapped to the positive range, and zero is mapped to a predefined nibble.</param>
    /// <returns>A tuple containing the following: <list type="bullet"> <item> <description> A <see cref="byte"/> representing
    /// the nibble value for the analog channel output. </description> </item> <item> <description> A <see cref="bool"/>
    /// indicating whether the nibble corresponds to the zero value. <see langword="true"/> if the nibble represents
    /// zero; otherwise, <see langword="false"/>. </description> </item> </list></returns>
    private (byte setValue_Nibble, bool zeroSet) SetOutput_AnalogChannel(float value)
    {
        // MK4: ZEROVALUE_NIBBLE = 0x08, RANGE_POS_OFFSET = 0x08
        // value <  0:  7 6 5 4 3 2 1                    RANGE_NEG: 0x07
        // value == 0:                0 8
        // value >  0:                    9 A B C D E F  RANGE_POS: 0x07

        const byte RANGE_POS_OFFSET = 0x08;
        const int RANGE_POS = 0x07;
        const int RANGE_NEG = 0x07;

        const float MIN_NEG_RANGE_THRESHOLD = -1f / RANGE_NEG;    // Minimum value for negative range
        const float MIN_POS_RANGE_THRESHOLD = 1f / RANGE_POS;     // Minimum value for positive range

        const byte ZEROVALUE_NIBBLE = 0x00;

        if (value <= MIN_NEG_RANGE_THRESHOLD)
        {
            float value_abs = Math.Min(0x07, -value * RANGE_NEG);
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else if (value >= MIN_POS_RANGE_THRESHOLD)
        {
            float value_abs = Math.Min(0x0F, (value * RANGE_POS) + RANGE_POS_OFFSET);
            byte setValue_nibble = (byte)(0x0F & (byte)(value_abs));

            return (setValue_nibble, false);
        }
        else
        {
            return (ZEROVALUE_NIBBLE, true);
        }
    }
}
