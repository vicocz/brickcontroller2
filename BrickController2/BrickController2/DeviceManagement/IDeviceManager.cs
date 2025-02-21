using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IDeviceManager : INotifyPropertyChanged
    {
        ObservableCollection<Device> Devices { get; }
        bool IsScanning { get; }
        bool IsBluetoothOn { get; }

        Task LoadDevicesAsync();
        Task<bool> ScanAsync(CancellationToken token);
        Task CreateDeviceAsync(DeviceType deviceType, string deviceName, string deviceAddress, byte[] deviceData);
        Task DeleteDeviceAsync(Device device);
        Task DeleteDevicesAsync();

        Device? GetDeviceById(string? Id);
    }
}
