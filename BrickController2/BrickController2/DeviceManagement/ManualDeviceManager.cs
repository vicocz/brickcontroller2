using System.Collections.Generic;
using System.Linq;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// This manager stores the needed data to build so called ManualDevices.
    /// ManualDevices cannot be scanned and so must be inserted manually.
    /// </summary>
    internal class ManualDeviceManager : IManualDeviceManager
    {
        public ManualDeviceManager(IEnumerable<IDeviceFactoryData> deviceFactoryDatas)
        {
            FactoryDataList = deviceFactoryDatas.ToArray();
        }

        public IEnumerable<IDeviceFactoryData> FactoryDataList { get; }
    }
}