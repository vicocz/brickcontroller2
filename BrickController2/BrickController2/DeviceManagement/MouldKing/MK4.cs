using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MK 4.0 Module
    /// </summary>
    internal class MK4 : MKBaseNible
    {
        public const string Device1 = "Device1";
        public const string Device2 = "Device2";
        public const string Device3 = "Device3";

        /// <summary>
        /// offset to position of first analog channel in base telegram
        /// </summary>
        private const int ChannelStartOffset = 3;

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

        /// <summary>
        /// manufacturerId to advertise
        /// </summary>
        protected override ushort ManufacturerId => MKProtocol.ManufacturerID;

        /// <summary>
        /// number of bytes containing channel values in base telegram
        /// -> channel 0..3 for all three devices
        /// </summary>
        protected override int BaseTelegram_ChannelBytesCount => 6;

        /// <summary>
        /// offset to position of first channel in base telegram
        /// </summary>
        protected override int BaseTelegram_ChannelStartOffset => 3;

        public MK4(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService)
          : base(name, address, deviceData, deviceRepository, bleService, mkPlatformService, MK4.GetChannelStartOffset(address), MK4.Telegram_Connect, MK4.Telegram_Base)
        {
        }

        public override DeviceType DeviceType => DeviceType.MK4;

        /// <summary>
        /// Number of channels for one MK4.0 device
        /// * channel 0..3 are analog 
        /// </summary>
        public override int NumberOfChannels => 4;

        /// <summary>
        /// Get reference to Base-Telegram for the given address
        /// </summary>
        /// <param name="address">address</param>
        /// <returns>reference to Base-Telegram</returns>
        private static int GetChannelStartOffset(string address)
        {
            return address switch
            {
                MK4.Device1 => MK4.ChannelStartOffset,
                MK4.Device2 => MK4.ChannelStartOffset + 2,
                MK4.Device3 => MK4.ChannelStartOffset + 4,
                _ => throw new ArgumentException("Illegal Argument", nameof(address))
            };
        }

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
                    bluetoothAdvertisingDeviceHandler = new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, MK4.ReconnectTimeSpan);
                }
                return bluetoothAdvertisingDeviceHandler;
            }
        }
    }
}
