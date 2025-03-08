using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public interface IStaticDeviceManager
    {
        IEnumerable<IStaticDeviceFactoryData> FactoryDataList { get; }
    }
}
