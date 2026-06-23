using BrickController2.DeviceManagement;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class DeviceEntry
    {
        public IDeviceFactoryData DeviceFactoryData { get; }
        public Device? ExistingDevice { get; }
        public bool Selected { get; set; }

        public DeviceEntry(IDeviceFactoryData deviceFactoryData, Device? instace)
        {
            DeviceFactoryData = deviceFactoryData;
            ExistingDevice = instace;
            Selected = instace != null;
        }
    }
}
