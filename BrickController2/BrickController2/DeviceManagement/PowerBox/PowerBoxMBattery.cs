using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;

namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// PowerBox 1 Channel
/// </summary>
internal class PowerBoxMBattery : PowerBoxBase, IDeviceType<PowerBoxMBattery>
{
    public const string Device = "Device";

    /// <summary>
    /// Telegram to connect to the PowerBox devices
    /// This telegram is sent on init and on reconnect conditions matching
    /// </summary>
    private static readonly byte[] Telegram_Connect_Device = [0xA4, 0xFE, 0x19, 0x80, 0x80, 0x80, 0x00, 0x5B];

    /// <summary>
    /// Base Telegram for PowerBox devices
    /// </summary>
    private static readonly byte[] Telegram_Base_Device = [0x40, 0xFE, 0x19, 0x00, 0x00, 0x00, 0x00, 0xBF];

    /// <summary>
    /// after this timespan and all channel's values equal to zero the connect telegram is sent
    /// </summary>
    private static readonly TimeSpan ReconnectTimeSpan = TimeSpan.FromSeconds(3);

    public PowerBoxMBattery(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IPowerBoxPlatformService powerboxPlatformService, PowerBoxDeviceManager powerboxDeviceManager)
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
