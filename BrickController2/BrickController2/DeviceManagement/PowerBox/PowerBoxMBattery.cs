using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// PowerBox M Battery with 1 Channel
/// </summary>
internal class PowerBoxMBattery : PowerBoxBaseByte, IDeviceType<PowerBoxMBattery>
{
    public const string Device = "Device";

    /// <summary>
    /// Telegram to connect to the PowerBox devices
    /// This telegram is sent on init and on reconnect conditions matching
    /// </summary>
    private static readonly byte[] Telegram_Connect_Device = [0xa4, 0xfe, 0x19, 0x80, 0x80, 0x80, 0x00, 0x5b];

    /// <summary>
    /// Base Telegram for PowerBox devices
    /// </summary>
    private static readonly byte[] Telegram_Base_Device = [0x40, 0xfe, 0x19, 0x00, 0x00, 0x00, 0x00, 0xbf];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    public PowerBoxMBattery(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IPowerBoxPlatformService powerboxPlatformService, IPowerBoxDeviceManager powerboxDeviceManager)
      : base(name, address, deviceData, deviceRepository, bleService, powerboxPlatformService, powerboxDeviceManager, Telegram_Connect_Device, Telegram_Base_Device)
    {
    }

    public static DeviceType Type => DeviceType.PowerBoxMBattery;

    public static string TypeName => "PowerBox M Battery";

    public override DeviceType DeviceType => Type;

    /// <summary>
    /// Gets the number of channels supported by the device.
    /// <remarks><list type="bullet">
    /// <item><description>Channel 0: real existing channel</description></item> 
    /// </list></remarks>
    /// </summary>
    public override int NumberOfChannels => 1;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => PowerBoxProtocol.ManufacturerID;

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        // PowerBoxMBattery needs a BluetoothAdvertiser per module
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, PowerBoxMBattery.ReconnectTimeSpan);
    }

    /// <summary>
    /// Updates a specific byte in the telegram buffer and returns whether the value was changed.
    /// </summary>
    /// <remarks>This method modifies the telegram buffer by updating the specified byte. The operation is 
    /// thread-safe and ensures exclusive access to the buffer during the
    /// update.</remarks>
    /// <param name="byteOffset">The zero-based index of the byte in the telegram buffer to modify.</param>
    /// <param name="setValue_byte">The value to set.</param>
    /// <returns><see langword="true"/> if the byte in the telegram buffer was modified; otherwise, <see langword="false"/> if
    /// the value remained unchanged.</returns>
    protected override bool SetChannelValue(int byteOffset, byte setValue_byte)
    {
        lock (_outputLock)
        {
            byte originValue1 = _telegram_Base[byteOffset];
            byte originValue2 = _telegram_Base[byteOffset + 1];

            _telegram_Base[byteOffset] = setValue_byte;
            _telegram_Base[byteOffset + 1] = setValue_byte;         // very special: bytes are duplicated in the datagram

            return _telegram_Base[byteOffset] != originValue1 || 
                _telegram_Base[byteOffset + 1] != originValue2;
        }
    }

    /// <summary>
    /// Converts a floating-point value into a byte representation for an analog channel output.
    /// </summary>
    /// <remarks>The method maps the input value to a byte representation based on predefined ranges for
    /// positive, negative, and zero values. The zero value is represented by a specific byte constant. The caller can
    /// use the returned boolean to determine if the byte corresponds to the zero value.</remarks>
    /// <param name="value">The floating-point value to be converted. Negative values are mapped to the negative range, positive values are
    /// mapped to the positive range, and zero is mapped to a predefined byte.</param>
    /// <returns>A tuple containing the following: <list type="bullet"> <item> <description> A <see cref="byte"/> representing
    /// the byte value for the analog channel output. </description> </item> <item> <description> A <see cref="bool"/>
    /// indicating whether the byte corresponds to the zero value. <see langword="true"/> if the byte represents
    /// zero; otherwise, <see langword="false"/>. </description> </item> </list></returns>
    protected (byte setValue_Byte, bool zeroSet) SetOutput_AnalogChannel(float value)
    {
        // value <  0:  7f 6e 5d 4c 3b 2a 19                          RANGE_NEG: 0x07
        // value == 0:                       00                       ZEROVALUE
        // value >  0:                          91 a2 b3 c4 d5 e6 f7  RANGE_POS: 0x07

        const int RANGE_POS = 0x07;
        const int RANGE_NEG = 0x07;

        const float MIN_NEG_RANGE_THRESHOLD = -1f / RANGE_NEG;    // Minimum value for negative range
        const float MIN_POS_RANGE_THRESHOLD = 1f / RANGE_POS;     // Minimum value for positive range

        const byte ZEROVALUE = 0x00;

        if (value <= MIN_NEG_RANGE_THRESHOLD)
        {
            byte value_abs = (byte)Math.Min(0x07, -value * RANGE_NEG);
            byte setValue_byte = (byte)((value_abs << 4) + value_abs + 8);

            return (setValue_byte, false);
        }
        else if (value >= MIN_POS_RANGE_THRESHOLD)
        {
            byte value_abs = (byte)Math.Min(0x07, value * RANGE_POS);
            byte setValue_byte = (byte)(((value_abs + 8) << 4) + value_abs);

            return (setValue_byte, false);
        }
        else
        {
            return (ZEROVALUE, true);
        }
    }

    /// <summary>
    /// Processes the value for the specified analog channel and returns the processed result.
    /// </summary>
    /// <param name="channelNo">The channel number to process. Valid value is 0.</param>
    /// <param name="value">The input value to be processed for the specified channel.</param>
    /// <returns>A tuple containing the processed value and a flag indicating the success of the operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not the valid channel number 0.</exception>
    protected override (byte value, bool flag) ProcessChannelValue(int channelNo, float value) => channelNo switch
    {
        0 => SetOutput_AnalogChannel(value),
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };
}
