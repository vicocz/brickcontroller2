using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.InfraredDevice
{
    internal interface IInfraredDeviceManager
    {
        Task<DeviceConnectionResult> ConnectDevice(InfraredDevice device);
        Task DisconnectDevice(InfraredDevice device);

        void SetOutput(InfraredDevice device, int channel, int value);
    }
}
