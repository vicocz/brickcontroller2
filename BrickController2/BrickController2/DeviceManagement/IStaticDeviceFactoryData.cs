using BrickController2.Settings;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public interface IStaticDeviceFactoryData
    {
        DeviceType DeviceType { get; }
        string Name { get; }
        string Address { get; }
        byte[] DeviceData { get; }
        IEnumerable<NamedSetting> Settings { get; }
    }
}
