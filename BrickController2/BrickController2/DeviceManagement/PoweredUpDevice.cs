using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Maui.Controls;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : ControlPlusDevice
    {
        public PoweredUpDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;
        public override ImageSource Image => ResourceHelper.GetImageResource("poweredup_image.png");
        public override ImageSource SmallImage => ResourceHelper.GetImageResource("poweredup_image_small.png");
        public override int NumberOfChannels => 2;
    }
}
