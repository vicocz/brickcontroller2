using BrickController2.DeviceManagement.Vendors;
using BrickController2.Settings;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public class DeviceFactoryData<TVendor, TDevice> : IDeviceFactoryData
        where TDevice : Device, IDeviceType<TDevice>
         where TVendor : Vendor<TVendor>
    {
        public DeviceFactoryData(TVendor vendor, string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings)
        {
            Vendor = vendor;
            Name = name;
            Address = address;
            DeviceData = deviceData;
            Settings = settings;
        }

        public DeviceType DeviceType => TDevice.Type;

        public TVendor Vendor { get; }
        public string Name { get; }
        public string Address { get; }
        public byte[] DeviceData { get; }
        public IEnumerable<NamedSetting> Settings { get; }

        public string DeviceTypeName => TDevice.TypeName;
        public string VendorName => Vendor.VendorName;
    }
}
