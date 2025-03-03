using System.Collections.Generic;
using System.Linq;
using BrickController2.Helpers;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// This manager stores the needed data to build so called StaticDevices.
    /// StaticDevices cannot be scanned and so must be inserted manually.
    /// </summary>
    internal class StaticDeviceManager : NotifyPropertyChangedSource, IStaticDeviceManager
    {
        public StaticDeviceManager(IEnumerable<IStaticDeviceFactoryData> staticDeviceFactoryDatas)
        {
            FactoryDataList = staticDeviceFactoryDatas.ToArray();
        }

        public IEnumerable<IStaticDeviceFactoryData> FactoryDataList { get; }
    }
}