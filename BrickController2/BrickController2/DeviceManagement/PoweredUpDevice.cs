using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : ControlPlusDevice
    {
        public PoweredUpDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ILogger<PoweredUpDevice> logger)
            : base(name, address, deviceRepository, bleService, logger)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;
        public override int NumberOfChannels => 2;
    }
}
