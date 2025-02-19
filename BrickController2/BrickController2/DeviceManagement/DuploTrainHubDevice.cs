using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class DuploTrainHubDevice : ControlPlusDevice
    {
        private static readonly ImageSource image = ResourceHelper.GetImageResource("duplotrainhub_image.png");
        private static readonly ImageSource smallImage = ResourceHelper.GetImageResource("duplotrainhub_image_small.png");

        public DuploTrainHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.DuploTrainHub;
        public override ImageSource Image => DuploTrainHubDevice.image;
        public override ImageSource SmallImage => DuploTrainHubDevice.smallImage;
        public override int NumberOfChannels => 1;
    }
}
