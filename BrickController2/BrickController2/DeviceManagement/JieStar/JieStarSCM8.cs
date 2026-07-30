using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// JIESTAR 8 Channel Smart Creative Module (SCM)
/// </summary>
internal class JieStarSCM8 : JieStarBase, IDeviceType<JieStarSCM8>
{
    public const string Device1 = "Device1";
    public const string Device2 = "Device2";
    public const string Device3 = "Device3";

    /// <summary>
    /// Telegram to connect to the SCM8 device(s)
    /// This telegram is sent on init and on reconnect conditions matching
    /// </summary>
    private static readonly byte[] Telegram_Connect = [0xA4, 0x34, 0x17, 0x00, 0x00, 0x00, 0x00, 0x5B];

    /// <summary>
    /// Base Telegram for SCM8 device 1
    /// </summary>
    private static readonly byte[] Telegram_Base_Device_1 = [0x41, 0x34, 0x17, 0x00, 0x00, 0x00, 0x00, 0xbf];

    /// <summary>
    /// Base Telegram for SCM8 device 2
    /// </summary>
    private static readonly byte[] Telegram_Base_Device_2 = [0x42, 0x34, 0x17, 0x00, 0x00, 0x00, 0x00, 0xbe];

    /// <summary>
    /// Base Telegram for SCM8 device 3
    /// </summary>
    private static readonly byte[] Telegram_Base_Device_3 = [0x43, 0x34, 0x17, 0x00, 0x00, 0x00, 0x00, 0xbd];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    public JieStarSCM8(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IJieStarPlatformService jieStarPlatformService, IJieStarDeviceManager jieStarDeviceManager)
      : base(name, address, deviceData, deviceRepository, bleService, jieStarPlatformService, jieStarDeviceManager, JieStarSCM8.Telegram_Connect, GetTelegramBase(address), JieStarProtocol.CTXValue2)
    {
    }

    public static DeviceType Type => DeviceType.JieStarSCM8;

    public static string TypeName => "JIESTAR SCM 8";

    public override DeviceType DeviceType => Type;

    /// <summary>
    /// Gets the number of channels supported by the device.
    /// <remarks><list type="bullet">
    /// <item><description>Channel 0..7: real existing channel</description></item> 
    /// </list></remarks>
    /// </summary>
    public override int NumberOfChannels => 8;

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
        // JIESTAR SCM 8 needs a BluetoothAdvertiser per module
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, JieStarSCM8.ReconnectTimeSpan);
    }


    /// <summary>
    /// Processes the value for the specified analog channel and returns the processed result.
    /// </summary>
    /// <param name="channelNo">The channel number to process. Valid values are 0, 1, 2, 3, 4, 5, 6, or 7.</param>
    /// <param name="value">The input value to be processed for the specified channel.</param>
    /// <returns>A tuple containing the processed value and a flag indicating the success of the operation.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="channelNo"/> is not one of the valid channel numbers (0, 1, 2, 3, 4, 5, 6, or 7).</exception>
    protected override (byte value, bool flag) ProcessChannelValue(int channelNo, float value) => channelNo switch
    {
        >= 0 and <= 7 => SetOutput_AnalogChannel(value),
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };

    /// <summary>
    /// Get reference to Base-Telegram for the given address
    /// </summary>
    /// <param name="address">address</param>
    /// <returns>reference to Base-Telegram</returns>
    private static byte[] GetTelegramBase(string address)
    {
        return address switch
        {
            JieStarSCM8.Device1 => Telegram_Base_Device_1,
            JieStarSCM8.Device2 => Telegram_Base_Device_2,
            JieStarSCM8.Device3 => Telegram_Base_Device_3,
            _ => throw new ArgumentException("Illegal Argument", nameof(address))
        };
    }
}
