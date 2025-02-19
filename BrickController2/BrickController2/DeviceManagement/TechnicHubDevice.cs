using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class TechnicHubDevice : ControlPlusDevice
    {
        private static readonly ImageSource image = ResourceHelper.GetImageResource("technichub_image.png");
        private static readonly ImageSource smallImage = ResourceHelper.GetImageResource("technichub_image_small.png");

        public TechnicHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicHub;
        public override ImageSource Image => TechnicHubDevice.image;
        public override ImageSource SmallImage => TechnicHubDevice.smallImage;
        public override int NumberOfChannels => 4;

    }
}
