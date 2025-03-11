using System;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using System.Threading.Tasks;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;
using BrickController2.UI.Commands;
using System.Threading;
using BrickController2.UI.Services.Translation;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDialogService _dialogService;

        private bool _isDisappearing = false;

        public DeviceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IBluetoothLEService bluetoothLEService,
            IDeviceManager deviceManager,
            IDialogService dialogService) 
            : base(navigationService, translationService)
        {
            DeviceManager = deviceManager;
            _dialogService = dialogService;

#if DEBUG
            // JK: to allow development on windows this is enabled
            IsBLEAdvertisingSupported = true;
#else
            IsBLEAdvertisingSupported = bluetoothLEService.IsBluetoothLEAdvertisingSupported;
#endif


            ScanCommand = new SafeCommand(async () => await ScanAsync(), () => !DeviceManager.IsScanning);
            ShowManualDeviceListPageCommand = new SafeCommand(async () => await ShowManualDeviceListPageAsync(), () => !DeviceManager.IsScanning);
            DeviceTappedCommand = new SafeCommand<Device>(async device => await NavigationService.NavigateToAsync<DevicePageViewModel>(new NavigationParameters(("device", device))));
            DeleteDeviceCommand = new SafeCommand<Device>(async device => await DeleteDeviceAsync(device));
            DeviceSettingsCommand = new SafeCommand<Device>(OpenDeviceSettingsAsync);
        }

        public IDeviceManager DeviceManager { get; }

        public ICommand ScanCommand { get; }
        public ICommand ShowManualDeviceListPageCommand { get; }
        public ICommand DeviceTappedCommand { get; }
        public ICommand DeleteDeviceCommand { get; }
        public ICommand DeviceSettingsCommand { get; }

        public bool IsBLEAdvertisingSupported { get; }

        public override void OnAppearing()
        {
            _isDisappearing = false;
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            _isDisappearing = true;
            base.OnDisappearing();
        }

        private async Task DeleteDeviceAsync(Device device)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteDevice")} '{device.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    DisappearingToken))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await DeviceManager.DeleteDeviceAsync(device),
                        Translate("Deleting"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task OpenDeviceSettingsAsync(Device device)
        {
            try
            {
                await NavigationService.NavigateToAsync<DeviceSettingsPageViewModel>(new (device));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ShowManualDeviceListPageAsync()
        {
            try
            {
                await NavigationService.NavigateToAsync<ManualDeviceListPageViewModel>(new());
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ScanAsync()
        {
            if (!DeviceManager.IsBluetoothOn)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("BluetoothIsTurnedOff"),
                    Translate("Ok"),
                    DisappearingToken);
            }

            var percent = 0;
            var scanResult = true;
            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    if (!_isDisappearing)
                    {
                        using (var cts = new CancellationTokenSource())
                        using (DisappearingToken.Register(() => cts.Cancel()))
                        {
                            Task<bool>? scanTask = null;
                            try
                            {
                                scanTask = DeviceManager.ScanAsync(cts.Token);

                                while (!token.IsCancellationRequested && percent <= 100 && !scanTask.IsCompleted)
                                {
                                    progressDialog.Percent = percent;
                                    await Task.Delay(100, token);
                                    percent += 1;
                                }
                            }
                            catch (Exception)
                            { }

                            cts.Cancel();

                            if (scanTask is not null)
                            {
                                scanResult = await scanTask;
                            }
                        }
                    }
                },
                Translate("Scanning"),
                Translate("SearchingForDevices"),
                Translate("Cancel"));

            if (!scanResult && !_isDisappearing)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("ErrorDuringScanning"),
                    Translate("Ok"),
                    CancellationToken.None);
            }
        }
    }
}
