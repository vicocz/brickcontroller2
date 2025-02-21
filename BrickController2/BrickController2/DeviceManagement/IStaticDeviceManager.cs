using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IStaticDeviceManager : INotifyPropertyChanged
    {
        ObservableCollection<IStaticDeviceFactoryData> FactoryDatas { get; }
    }
}
