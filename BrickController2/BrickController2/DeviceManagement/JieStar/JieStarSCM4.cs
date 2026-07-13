using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// JIESTAR 4 Channel Smart Creative Module (SCM)
/// </summary>
internal class JieStarSCM4 : JieStarBase, IDeviceType<JieStarSCM4>
{
    public const string Device1 = "Device1";
    public const string Device2 = "Device2";
    public const string Device3 = "Device3";

    /// <summary>
    /// Telegram to connect to the SCM4 devices
    /// This telegram is sent on init and on reconnect conditions matching
    /// </summary>
    private static readonly byte[] Telegram_Connect_Device = [0xa4, 0x1d, 0x74, 0x80, 0x80, 0x80, 0x80, 0x5b];

    /// <summary>
    /// Base Telegram for SCM4 devices
    /// </summary>
    private static readonly byte[] Telegram_Base_Device = [0x40, 0x1d, 0x74, 0x80, 0x80, 0x80, 0x80, 0xbf];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    public JieStarSCM4(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IJieStarPlatformService jieStarPlatformService, JieStarDeviceManager jieStarDeviceManager)
      : base(name, address, deviceData, deviceRepository, bleService, jieStarPlatformService, jieStarDeviceManager, Telegram_Connect_Device, Telegram_Base_Device, GetCTXValue2(address))
    {
    }

    public static DeviceType Type => DeviceType.JieStarSCM4;

    public static string TypeName => "JIESTAR SCM 4";

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
        // JIESTAR SCM 4 needs a BluetoothAdvertiser per module
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
        >= 0 and <= 3 => SetOutput_AnalogChannel(value),
        _ => throw new ArgumentException($"Illegal Argument \"{channelNo}\"", nameof(channelNo))
    };

    /// <summary>
    /// Get reference to Base-Telegram for the given address
    /// </summary>
    /// <param name="address">address</param>
    /// <returns>reference to Base-Telegram</returns>
    private static byte GetCTXValue2(string address)
    {
        return address switch
        {
            JieStarSCM4.Device1 => JieStarProtocol.CTXValue2,
            JieStarSCM4.Device2 => JieStarProtocol.CTXValue2 + 1,
            JieStarSCM4.Device3 => JieStarProtocol.CTXValue2 + 2,
            _ => throw new ArgumentException("Illegal Argument", nameof(address))
        };
    }
}
