using System.Collections.Generic;
using System.ComponentModel;

namespace BrickController2.DeviceManagement
{
    public interface IStaticDeviceManager : INotifyPropertyChanged
    {
        IEnumerable<IStaticDeviceFactoryData> FactoryDataList { get; }
    }
}
