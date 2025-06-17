using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.MouldKing
{
    /// <summary>
    /// MK 4.0 Module
    /// </summary>
    internal class MK4 : MKBaseNibble, IDeviceType<MK4>
    {
        public const string Device1 = "Device1";
        public const string Device2 = "Device2";
        public const string Device3 = "Device3";

    /// <summary>
    /// Telegram to connect to the MK4.0 device(s)
    /// This telegram is sent on init and on reconnect conditions matching
    /// </summary>
    private static readonly byte[] Telegram_Connect = new byte[] { 0xAD, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x4F, 0x52 };

    /// <summary>
    /// Base Telegram for MK4.0
    /// -> this telegram handles all three MK4.0 devices
    /// * channels 0..3 for Device1 start at offset 3 and are analog channels
    /// * channels 0..3 for Device2 start at offset 5 and are analog channels
    /// * channels 0..3 for Device3 start at offset 7 and are analog channels
    /// </summary>
    private static readonly byte[] Telegram_Base = new byte[] { 0x7D, 0x7B, 0xA7, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x82 };

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    /// <summary>
    /// all MK4.0 modules share the same BluetoothAdvertisingDeviceHandler
    /// </summary>
    private static BluetoothAdvertisingDeviceHandler? bluetoothAdvertisingDeviceHandler;

    public MK4(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
      : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, GetInstanceNo(address), Telegram_Connect, Telegram_Base)
    {
    }

        public override DeviceType DeviceType => Type;

        public static DeviceType Type => DeviceType.MK4;

        public static string TypeName => "MK 4.0";

    /// <summary>
    /// Number of channels for one MK4.0 device
    /// * channel 0..3 are analog 
    /// </summary>
    public override int NumberOfChannels => 4;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        lock (typeof(MK4)) // lock type
        {
            if (bluetoothAdvertisingDeviceHandler == null)
            {
                // all MK4.0 modules share the same BluetoothAdvertisingDeviceHandler
                bluetoothAdvertisingDeviceHandler = new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, ReconnectTimeSpan);
            }
            return bluetoothAdvertisingDeviceHandler;
        }
    }

    /// <summary>
    /// Returns a function that processes a floating-point input value and produces a tuple containing the channel
    /// offset and the result of the analog channel output operation.
    /// </summary>
    /// <remarks>The returned function is specific to the provided channel number and uses predefined
    /// parameters  to calculate the analog channel output. The caller must ensure that the channel number is valid 
    /// to avoid exceptions.</remarks>
    /// <param name="channelNo">The channel number for which the processing function is requested. Valid values are 0, 1, 2, or 3.</param>
    /// <returns>A function that takes a <see langword="float"/> input value and returns a tuple containing: - An <see
    /// langword="int"/> representing the channel offset. - A nested tuple containing:   - A <see langword="byte"/>
    /// representing the output value.   - A <see langword="bool"/> indicating the success or failure of the
    /// operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not one of the valid channel numbers (0, 1, 2, or 3).</exception>
    protected override Func<float, (byte, bool)> CreateSetChannel(int channelNo)
    {
        return channelNo switch
        {
            0 => (float value) => SetOutput_AnalogChannel(value),
            1 => (float value) => SetOutput_AnalogChannel(value),
            2 => (float value) => SetOutput_AnalogChannel(value),
            3 => (float value) => SetOutput_AnalogChannel(value),
            _ => throw new ArgumentException("Illegal Argument", nameof(channelNo))
        };
    }

    private (byte, bool) SetOutput_AnalogChannel(float value)
    {
        // MK4: ZeroValueNibble = 0x08, Range_pos_Offset = 0x08
        // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
        // value == 0:                0 8
        // value >  0:                    9 A B C D E F  range_pos: 0x07
        const byte ZeroValueNibble = 0x08;
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

    /// <summary>
    /// Determines the instance number associated with the specified device address.
    /// </summary>
    /// <param name="address">The address of the device. Must match one of the predefined device addresses.</param>
    /// <returns>An integer representing the zero based instance number of the device.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="address"/> does not match any predefined device address.</exception>
    private static int GetInstanceNo(string address)
    {
        return address switch
        {
            Device1 => 0,
            Device2 => 1,
            Device3 => 2,
            _ => throw new ArgumentException("Illegal Argument", nameof(address))
        };
    }
}
