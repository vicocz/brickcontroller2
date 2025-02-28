using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MK 6.0 Module
    /// </summary>
    internal class MK6 : MKBaseByte
    {
        /// <summary>
        /// ManufacturerID for MK
        /// </summary>
        public const ushort ManufacturerID = 0xFFF0;

        public const string Device1 = "Device1";
        public const string Device2 = "Device2";
        public const string Device3 = "Device3";

        /// <summary>
        /// Telegram connect to MK6.0 (switch MK6.0 to Bluetooth mode)
        /// </summary>
        private static readonly byte[] Telegram_Connect = new byte[] { 0x6D, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x92, };

        /// <summary>
        /// Base Telegram for Device 1
        /// </summary>
        private static readonly byte[] Telegram_Base_Device_1 = new byte[] { 0x61, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x9E };

        /// <summary>
        /// Base Telegram for Device 2
        /// </summary>
        private static readonly byte[] Telegram_Base_Device_2 = new byte[] { 0x62, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x9D };

        /// <summary>
        /// Base Telegram for Device 3
        /// </summary>
        private static readonly byte[] Telegram_Base_Device_3 = new byte[] { 0x63, 0x7B, 0xA7, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x9C };

        public MK6(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
          : base(name, address, deviceData, deviceRepository, bleService, MK6.ManufacturerID, 6, MK6.Telegram_Connect, MK6.GetTelegramBase(address))
        {
        }

        public override DeviceType DeviceType => DeviceType.MK6;

        /// <summary>
        /// Get reference to Base-Telegram for the given address
        /// </summary>
        /// <param name="address">address</param>
        /// <returns>reference to Base-Telegram</returns>
        private static byte[] GetTelegramBase(string address)
        {
            return address switch
            {
                MK6.Device3 => Telegram_Base_Device_3,
                MK6.Device2 => Telegram_Base_Device_2,
                MK6.Device1 => Telegram_Base_Device_1,
                _ => throw new ArgumentException("Illegal Argument", nameof(address))
            };
        }

        /// <summary>
        /// Get or create BluetoothAdvertiser
        /// </summary>
        /// <returns>Instance of BluetoothAdvertiser</returns>
        protected override BluetoothAdvertiser GetBluetoothAdvertiser()
        {
            // MK6.0 needs a BluetoothAdvertiser for each module
            return new BluetoothAdvertiser(_bleService, _manufacturerId, TryGetTelegram);
        }
    }
}
