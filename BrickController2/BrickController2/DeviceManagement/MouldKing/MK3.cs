using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK 3.0 Module
/// </summary>
internal class MK3 : MKBaseByte, IDeviceType<MK3>
{
    public const string Device = "Device";

    /// <summary>
    /// Telegram connect to MK3.0 (switch MK3.0 to Bluetooth mode)
    /// </summary>
    private static readonly byte[] Telegram_Connect = [0xaa, 0x7b, 0x7a, 0x00, 0x00, 0x00, 0x00, 0x55];

    /// <summary>
    /// Base Telegram
    /// </summary>
    private static readonly byte[] Telegram_Base_Device = [0x66, 0x7b, 0xa7, 0x80, 0x80, 0x80, 0x80, 0x99];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

    /// <summary>
    /// offset to position of first channel in base telegram
    /// </summary>
    protected override int BaseTelegram_ChannelStartOffset => 3;

    private bool _function1Enabled;
    private bool _function2Enabled;

    public MK3(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService, IMouldKingDeviceManager mkDeviceManager)
      : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, mkDeviceManager, 3, MK3.Telegram_Connect, MK3.Telegram_Base_Device)
    {
        InitDevice();
    }

    public static DeviceType Type => DeviceType.MK3;

    public static string TypeName => "MK 3.0";

    public override DeviceType DeviceType => Type;

    public override int NumberOfChannels => 4;

    /// <summary>
    /// Determines whether the specified channel number corresponds to a virtual channel.
    /// </summary>
    /// <param name="channelNo">The channel number to evaluate.</param>
    /// <returns><see langword="true"/> if the specified channel number is a virtual channel;  otherwise, <see
    /// langword="false"/>. </returns>
    protected override bool IsVirtualChannel(int channelNo) => channelNo switch
    {
        0 => false,
        1 => false,
        2 => true,
        3 => true,
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };

    /// <summary>
    /// Processes the value for a specified channel and returns the processed result along with a status flag.
    /// </summary>
    /// <returns>A tuple containing the processed byte value and a boolean flag indicating the success or status of the
    /// operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not within the valid range of 0 through 3.</exception>
    protected override (byte value, bool flag) ProcessChannelValue(int channelNo, float value) => channelNo switch
    {
        0 => SetOutput_AnalogChannel(value),
        1 => SetOutput_AnalogChannel(value),
        2 => SetOutput_Function1Channel(value),
        3 => SetOutput_Function2Channel(value),
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        // MK3.0 needs a BluetoothAdvertiser per module
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, MK3.ReconnectTimeSpan);
    }

    private (byte setValue_Byte, bool zeroSet) SetOutput_Function1Channel(float value)
    {
        _function1Enabled = value != 0;

        return SetOutput_Channel_3_4(value);
    }

    private (byte setValue_Byte, bool zeroSet) SetOutput_Function2Channel(float value)
    {
        _function2Enabled = value != 0;

        return SetOutput_Channel_3_4(value);
    }

    private (byte setValue_Byte, bool zeroSet) SetOutput_Channel_3_4(float value)
    {
        byte setValue_byte = 0x80;
        bool zeroSet = true;

        if (_function1Enabled || _function2Enabled)
        {
            setValue_byte = (byte)((_function1Enabled ? (byte)0xf0 : (byte)0x00) | (_function2Enabled ? (byte)0x0f : (byte)0x00));
            zeroSet = false;
        }

        _bluetoothAdvertisingDeviceHandler.SetChannelState(2, zeroSet);
        _bluetoothAdvertisingDeviceHandler.SetChannelState(3, zeroSet);

        bool valueChanged = false;

        lock (_outputLock) // ensure that the channel values are set atomically
        {
            valueChanged |= SetChannelValue(GetTargetPosition(2), setValue_byte);
            valueChanged |= SetChannelValue(GetTargetPosition(3), setValue_byte);
        }

        return (0x00, valueChanged);
    }
}
