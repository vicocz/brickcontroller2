using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.PowerFunctions
{
    internal interface IPowerFunctionsManager
    {
        Task<DeviceConnectionResult> ConnectDevice(PowerFunctionsDevice device);
        Task DisconnectDevice(PowerFunctionsDevice device);

        void SetOutput(PowerFunctionsDevice device, int channel, int value);
    }
}
