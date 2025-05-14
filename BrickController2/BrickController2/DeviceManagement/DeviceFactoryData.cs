using BrickController2.Settings;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public class DeviceFactoryData : IDeviceFactoryData
    {
        public DeviceFactoryData(DeviceType deviceType, string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings)
        {
            DeviceType = deviceType;
            Name = name;
            Address = address;
            DeviceData = deviceData;
            Settings = settings;
        }

        public DeviceType DeviceType { get; }
        public string Name { get; }
        public string Address { get; }
        public byte[] DeviceData { get; }
        public IEnumerable<NamedSetting> Settings { get; }
    }
}
