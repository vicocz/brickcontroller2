using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class TechnicHubDevice : ControlPlusDevice
    {
        public TechnicHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicHub;
        public override ImageSource Image => ResourceHelper.GetImageResource("technichub_image.png");
        public override ImageSource SmallImage => ResourceHelper.GetImageResource("technichub_image_small.png");
        public override int NumberOfChannels => 4;

    }
}
