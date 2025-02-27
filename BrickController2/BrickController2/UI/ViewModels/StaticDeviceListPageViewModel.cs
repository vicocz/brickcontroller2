using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class StaticDeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;

        public StaticDeviceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IStaticDeviceManager staticDeviceManager,
            IDialogService dialogService) 
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;

            foreach (var item in staticDeviceManager.FactoryDatas)
            {
                if (deviceManager.Devices.FirstOrDefault(d => d.DeviceType == item.DeviceType && d.Address == item.Address) == null)
                {
                    FactoryDatas.Add(item);
                }
            }
        }

        public ObservableCollection<IStaticDeviceFactoryData> FactoryDatas { get; } = new ObservableCollection<IStaticDeviceFactoryData>();
        public ObservableCollection<object> SelectedFactoryDatas { get; } = new ObservableCollection<object>(); // generiec Type object is a workaround: https://github.com/dotnet/maui/issues/23358

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override async void OnDisappearing()
        {
            await CreateDevicesAsync();

            base.OnDisappearing();
        }

        private async Task CreateDevicesAsync()
        {
            foreach (IStaticDeviceFactoryData item in SelectedFactoryDatas)
            {
                await _deviceManager.CreateDeviceAsync(item.DeviceType, item.Name, item.Address, item.DeviceData);
            }
        }
    }
}
