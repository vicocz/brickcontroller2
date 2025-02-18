using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
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
        public override ImageSource Image => ResourceHelper.GetImageResource("boost_image.png");
        public override ImageSource SmallImage => ResourceHelper.GetImageResource("boost_image_small.png");

        public override int NumberOfChannels => 4;
    }
}
