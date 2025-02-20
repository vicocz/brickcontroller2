using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MK 6.0 Module
    /// </summary>
    internal class MK6 : MKBaseByte
    {
        #region Constants
        /// <summary>
        /// ManufacturerID for MK
        /// </summary>
        public const ushort ManufacturerID = 0xFFF0;

        public const string Device1 = "Device1";
        public const string Device2 = "Device2";
        public const string Device3 = "Device3";

        /// <summary>
        /// Telegram wich is sent to connect to MK6.0
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
        #endregion

        #region Fields
        #endregion
        #region Properties
        public override DeviceType DeviceType => DeviceType.MK6;
        #endregion

        #region Constructor
        public MK6(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
          : base(name, address, deviceData, deviceRepository, bleService, MK6.ManufacturerID, 6, MK6.Telegram_Connect, MK6.GetTelegramBase(address))
        {
        }
        #endregion

        #region GetTelegramBase(string address)
        /// <summary>
        /// Gets the Base-Telegram for the given address
        /// </summary>
        /// <param name="address">address</param>
        /// <returns>Base-Telegram</returns>
        private static byte[] GetTelegramBase(string address)
        {
            switch (address)
            {
                case MK6.Device3:
                    return Telegram_Base_Device_3;
                case MK6.Device2:
                    return Telegram_Base_Device_2;
                case MK6.Device1:
                default:
                    return Telegram_Base_Device_1;
            }
        }
        #endregion
    }
}
