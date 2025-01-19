using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
        public BoostDevice(
            string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService,
            ILogger<BoostDevice> logger)
            : base(name, address, deviceRepository, bleService, logger)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;
        public override int NumberOfChannels => 4;
    }
}
