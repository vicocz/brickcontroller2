using System.Collections.Generic;

namespace BrickController2.DeviceManagement
{
    public interface IManualDeviceManager
    {
        IEnumerable<IDeviceFactoryData> FactoryDataList { get; }
    }
}
