using System;
using System.Linq;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using System.Threading.Tasks;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;
using BrickController2.UI.Commands;
using System.Threading;
using BrickController2.UI.Services.Translation;
using System.Collections.ObjectModel;
using BrickController2.Helpers;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.ViewModels
{
    public class StaticDeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IStaticDeviceManager _staticDeviceManager;
        private readonly IDialogService _dialogService;

        private bool _isDisappearing = false;

        public StaticDeviceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IStaticDeviceManager staticDeviceManager,
            IDialogService dialogService) 
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _staticDeviceManager = staticDeviceManager;
            _dialogService = dialogService;

            foreach (var item in staticDeviceManager.FactoryDatas)
            {
                if (deviceManager.Devices.FirstOrDefault(d => d.DeviceType == item.DeviceType && d.Address == item.Address) == null)
                {
                    FactoryDatas.Add(item);
                }
            }
        }

        public ObservableCollection<IStaticDeviceFactoryData> FactoryDatas { get; } = new ObservableCollection<IStaticDeviceFactoryData>();
        public ObservableCollection<IStaticDeviceFactoryData> SelectedFactoryDatas { get; } = new ObservableCollection<IStaticDeviceFactoryData>();

        public override void OnAppearing()
        {
            _isDisappearing = false;
            base.OnAppearing();
        }

        public override async void OnDisappearing()
        {
            await CreateDevicesAsync();

            _isDisappearing = true;
            base.OnDisappearing();
        }

        private async Task CreateDevicesAsync()
        {
            foreach (var item in SelectedFactoryDatas)
            {
                await _deviceManager.CreateDeviceAsync(item.DeviceType, item.Name, item.Address, item.DeviceData);
            }
        }
    }
}
