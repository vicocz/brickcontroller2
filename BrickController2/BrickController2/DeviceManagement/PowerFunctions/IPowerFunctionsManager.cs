using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.PowerFunctions
{
    internal interface IPowerFunctionsManager
    {
        Task<DeviceConnectionResult> ConnectDevice(PowerFunctions device);
        Task DisconnectDevice(PowerFunctions device);

        void SetOutput(PowerFunctions device, int channel, int value);
    }
}
