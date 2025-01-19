using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;

namespace BrickController2.DeviceManagement
{
    internal class DuploTrainHubDevice : ControlPlusDevice
    {
        public DuploTrainHubDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ILogger<DuploTrainHubDevice> logger)
            : base(name, address, deviceRepository, bleService, logger)
        {
        }

        public override DeviceType DeviceType => DeviceType.DuploTrainHub;
        public override int NumberOfChannels => 1;
    }
}
