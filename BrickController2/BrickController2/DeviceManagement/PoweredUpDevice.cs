using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : ControlPlusDevice
    {
        private static readonly ImageSource image = ResourceHelper.GetImageResource("poweredup_image.png");
        private static readonly ImageSource smallImage = ResourceHelper.GetImageResource("poweredup_image_small.png");

        public PoweredUpDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;
        public override ImageSource Image => PoweredUpDevice.image;
        public override ImageSource SmallImage => PoweredUpDevice.smallImage;
        public override int NumberOfChannels => 2;
    }
}
