using BrickController2.Settings;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public class DeviceFactoryData<TDevice> : IDeviceFactoryData
        where TDevice : Device, IDeviceType<TDevice>
    {
        public DeviceFactoryData(string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings)
        {
            Name = name;
            Address = address;
            DeviceData = deviceData;
            Settings = settings;
        }

        public DeviceType DeviceType => TDevice.Type;
        public string Name { get; }
        public string Address { get; }
        public byte[] DeviceData { get; }
        public IEnumerable<NamedSetting> Settings { get; }

        public string DeviceTypeName => TDevice.TypeName;
        public string VendorName => TDevice.TypeName; //TODO vendor
    }
}
