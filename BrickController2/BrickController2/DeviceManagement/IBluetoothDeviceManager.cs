using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal interface IBluetoothDeviceManager : IDeviceScanner
    {
        Task<bool> IsBluetoothOnAsync();
    }
}
