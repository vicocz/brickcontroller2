using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK 5.0 Module
/// </summary>
internal class MK5 : MKBaseNibble, IDeviceType<MK5>
{
    public const string Device = "Device";

    private const byte TURRET_UNLOCKED_NIBBLE = 0x00;
    private const byte TURRET_LOCKED_NIBBLE = 0x02;

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

    /// <summary>
    /// Offset for turret rotation.
    /// * 0x00: turret is unlocked
    /// * 0x02: turret is locked
    /// </summary>
    private byte _turret_lock_Nibble = TURRET_UNLOCKED_NIBBLE;

    public MK5(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, 0, Telegram_Connect, Telegram_Base)
    {
    }

    public override DeviceType DeviceType => Type;

    public static DeviceType Type => DeviceType.MK5;

    public static string TypeName => "MK 5.0";

    /// <summary>
    /// Gets the number of channels supported by the device
    /// <remarks><list type="bullet">
    /// <item><description>Channel 0..3: real existing channel</description></item> 
    /// <item><description>Channel 4: virtual channel for locking the turret</description></item>
    /// </list></remarks>
    /// </summary>
    public override int NumberOfChannels => 5;

    public override Task<DeviceConnectionResult> ConnectAsync(bool reconnect, Action<Device> onDeviceDisconnected, IEnumerable<ChannelConfiguration> channelConfigurations, bool startOutputProcessing, bool requestDeviceInformation, CancellationToken token)
    {
        _turret_lock_Nibble = TURRET_UNLOCKED_NIBBLE; // reset turret lock nibble on connect

        return base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
    }

    /// <summary>
    /// ManufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    /// <inheritdoc/>>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler() =>
        // there is only one instance of MK5.0
        new(_bleService, ManufacturerId, TryGetTelegram, ReconnectTimeSpan);

    /// <summary>
    /// Determines whether the specified channel number corresponds to a virtual channel.
    /// </summary>
    /// <param name="channelNo">The channel number to evaluate.</param>
    /// <returns><see langword="true"/> if the specified channel number is a virtual channel;  otherwise, <see
    /// langword="false"/>. </returns>
    protected override bool IsVirtualChannel(int channelNo) => channelNo == 4;

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
        0 => ProcessChannelValue_AnalogChannel(value),                    // Tracks A + B forward, backward
        1 => ProcessChannelValue_AnalogChannel_Turret(value),             // Turret rotation
        2 => ProcessChannelValue_Shot(value),                             // shot canon
        3 => ProcessChannelValue_AnalogChannel(value),                    // turn on spot
        4 => ProcessChannelValue_Option_Turret_Lock(value),               // virtual channel: lock turret
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
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
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else
        {
            return (ZEROVALUE_NIBBLE, true);
        }
    }

    /// <summary>
    /// Calculates the nibble value and zero-set flag for the analog channel turret based on the specified input value.
    /// </summary>
    /// <remarks>The method processes the input value by inverting it for turret-specific requirements and
    /// calculates the appropriate nibble value  based on predefined positive and negative ranges. If the calculated
    /// nibble value is zero, the zero value nibble is applied.</remarks>
    /// <param name="value">The input value to be processed. Negative values represent the negative range, positive values represent the
    /// positive range,  and zero represents the neutral state.</param>
    /// <returns>A tuple containing: <list type="bullet"> <item> <description><c>setValue_nibble</c>: The calculated nibble value
    /// to be set for the turret.</description> </item> <item> <description><c>zeroSet</c>: A boolean flag indicating
    /// whether the zero value nibble is used (<see langword="true"/> if the zero value nibble is applied; otherwise,
    /// <see langword="false"/>).</description> </item> </list></returns>
    private (byte setValue_Nibble, bool zeroSet) ProcessChannelValue_AnalogChannel_Turret(float value)
    {
        // MK5: zeroValue_Nibble = 0x00/0x02, RANGE_POS_OFFSET = 0x08
        // value <  0:  7 6 5 4 3 2 1                    RANGE_NEG: 0x07
        // value == 0:                0
        // value >  0:                  9 A B C D E F    RANGE_POS: 0x07

        const byte RANGE_POS_OFFSET = 0x08;
        const int RANGE_POS = 0x07;
        const int RANGE_NEG = 0x07;
        const float MIN_NEG_RANGE_THRESHOLD = -1f / RANGE_NEG;    // Minimum value for negative range
        const float MIN_POS_RANGE_THRESHOLD = 1f / RANGE_POS;     // Minimum value for positive range

        byte zeroValue_Nibble = _turret_lock_Nibble; // <-- turretOffset is set by ProcessChannelValue_Option_Turret_Lock

        value *= -1; // invert value for turret

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
            return (zeroValue_Nibble, true);
        }
    }

    /// <summary>
    /// Determines the output value and status for firing a shot based on the provided input.
    /// </summary>
    /// <remarks>The method is designed to control the firing mechanism of a tank. When the input value is
    /// <c>0</c>, the output indicates no firing (<c>0x00</c>), and <c>zeroSet</c> is <see langword="true"/>.  For all
    /// other values, the output indicates a firing action (<c>0x0F</c>), and <c>zeroSet</c> is <see
    /// langword="false"/>.</remarks>
    /// <param name="value">The input value used to determine the output. Must be a floating-point number.</param>
    /// <returns>A tuple containing: <list type="bullet"> <item><description><c>setValue_nibble</c>: A byte representing the
    /// output value. Returns <c>0x00</c> if <paramref name="value"/> is <c>0</c>, or <c>0x0F</c>
    /// otherwise.</description></item> <item><description><c>zeroSet</c>: A boolean indicating whether the input value
    /// was zero. Returns <see langword="true"/> if <paramref name="value"/> is <c>0</c>; otherwise, <see
    /// langword="false"/>.</description></item> </list></returns>
    private static (byte setValue_nibble, bool zeroSet) ProcessChannelValue_Shot(float value)
    {
        const byte ZEROVALUE_NIBBLE = 0x00;
        const byte FIRE_SHOT_NIBBLE = 0x0F;

        if (value == 0)
        {
            return (ZEROVALUE_NIBBLE, true);
        }
        else
        {
            // Tank fires a shot when value is set to 0x0F
            return (FIRE_SHOT_NIBBLE, false);
        }
    }

    /// <summary>
    /// Configures the turret lock state based on the specified value.
    /// </summary>
    /// <remarks>The turret lock is managed as a virtual channel, and the method invokes the handler for the
    /// corresponding real turret channel.</remarks>
    /// <param name="value">A floating-point value representing the desired turret lock state.  A value of <see langword="0"/> unlocks the
    /// turret, while any other value locks the turret.</param>
    /// <returns>A tuple containing two elements: <list type="bullet"> <item> <description><c>setValue_nibble</c>: A byte value
    /// indicating the nibble used for the turret lock operation. Always returns <c>0x00</c>.</description> </item>
    /// <item> <description><c>zeroSet</c>: A boolean value indicating whether the turret lock state was successfully
    /// updated.</description> </item> </list></returns>
    private (byte setValue_nibble, bool zeroSet) ProcessChannelValue_Option_Turret_Lock(float value)
    {
        if (value == 0)
        {
            _turret_lock_Nibble = TURRET_UNLOCKED_NIBBLE; // turret is unlocked
        }
        else
        {
            _turret_lock_Nibble = TURRET_LOCKED_NIBBLE; // turret is locked
        }

        bool valueChanged = false;
        valueChanged |= SetChannelOutput(1, _storedValues[1]); // The turret lock is a virtual channel, so call handler for real turret channel

        // to clarify the concept:
        // inside a virtual channel handler multiple calls to _setChannel are allowed
        //valueChanged |= _setChannel[x](_storedValues[x]); 

        return (_turret_lock_Nibble, valueChanged);
    }
}
