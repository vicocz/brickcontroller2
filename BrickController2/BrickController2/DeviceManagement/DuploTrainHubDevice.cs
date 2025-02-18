using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class DuploTrainHubDevice : ControlPlusDevice
    {
        public DuploTrainHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.DuploTrainHub;
        public override ImageSource Image => ResourceHelper.GetImageResource("duplotrainhub_image.png");
        public override ImageSource SmallImage => ResourceHelper.GetImageResource("duplotrainhub_image_small.png");
        public override int NumberOfChannels => 1;
    }
}
