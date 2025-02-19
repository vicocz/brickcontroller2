using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
        private static readonly ImageSource image = ResourceHelper.GetImageResource("boost_image.png");
        private static readonly ImageSource smallImage = ResourceHelper.GetImageResource("boost_image_small.png");

        public BoostDevice(
            string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;
        public override ImageSource Image => BoostDevice.image;
        public override ImageSource SmallImage => BoostDevice.smallImage;

        public override int NumberOfChannels => 4;
    }
}
