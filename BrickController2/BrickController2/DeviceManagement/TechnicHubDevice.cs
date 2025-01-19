using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;

namespace BrickController2.DeviceManagement
{
    internal class TechnicHubDevice : ControlPlusDevice
    {
        public TechnicHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ILogger<TechnicHubDevice> logger)
            : base(name, address, deviceRepository, bleService, logger)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicHub;
        public override int NumberOfChannels => 4;

    }
}
