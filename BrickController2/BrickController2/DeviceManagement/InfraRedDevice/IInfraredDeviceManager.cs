using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.InfraredDevice
{
    internal interface IInfraredDeviceManager : IDeviceScanner
    {
        Task<DeviceConnectionResult> ConnectDevice(InfraredDevice device);
        Task DisconnectDevice(InfraredDevice device);

        void SetOutput(InfraredDevice device, int channel, int value);
    }
}
