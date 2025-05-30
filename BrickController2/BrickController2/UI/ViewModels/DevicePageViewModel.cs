using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Translation;
using Device = BrickController2.DeviceManagement.Device;
using static BrickController2.CreationManagement.ControllerDefaults;

namespace BrickController2.UI.ViewModels
{
    public class DevicePageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource? _connectionTokenSource;
        private Task? _connectionTask;
        private bool _reconnect = false;
        private bool _isDisappearing = false;

        public DevicePageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            Device = parameters.Get<Device>("device");
            BuWizzOutputLevel = Device.DefaultOutputLevel;
            BuWizz2OutputLevel = Device.DefaultOutputLevel;
            DeviceOutputs =  Enumerable
                .Range(0, Device.NumberOfChannels)
                .Select(channel => new DeviceOutputViewModel(navigationService, Device, channel))
                .ToArray();

            RenameCommand = new SafeCommand(async () => await RenameDeviceAsync());
            BuWizzOutputLevelChangedCommand = new SafeCommand<int>(outputLevel => SetBuWizzOutputLevel(outputLevel));
            BuWizz2OutputLevelChangedCommand = new SafeCommand<int>(outputLevel => SetBuWizzOutputLevel(outputLevel));
            ActivateShelfModeCommand = new SafeCommand(ActivateShelfModeCommandAsync,
                () => Device.DeviceState == DeviceState.Connected && Device.CanActivateShelfMode);
            ScanCommand = new SafeCommand(ScanAsync, () => CanExecuteScan);
            OpenDeviceSettingsPageCommand = new SafeCommand(async () => await navigationService.NavigateToAsync<DeviceSettingsPageViewModel>(new (Device)),
                () => CanOpenSettings);
        }

        public Device Device { get; }
        public bool IsBuWizzDevice => Device.DeviceType == DeviceType.BuWizz;
        public bool IsBuWizz2Device => Device.DeviceType == DeviceType.BuWizz2;
        public bool CanBePowerSource => Device.CanBePowerSource;
        public bool CanExecuteScan => Device.CanBePowerSource &&
            Device.DeviceState == DeviceState.Connected &&
            !_deviceManager.IsScanning;

        public bool CanOpenSettings => Device.HasSettings &&
            Device.DeviceState == DeviceState.Connected &&
            !_deviceManager.IsScanning;

        public bool IsServoOrStepperSupported => DeviceOutputs.Any(x => x.IsServoOrStepperSupported);

        public ICommand RenameCommand { get; }
        public ICommand BuWizzOutputLevelChangedCommand { get; }
        public ICommand BuWizz2OutputLevelChangedCommand { get; }
        public ICommand ActivateShelfModeCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand OpenDeviceSettingsPageCommand { get; }

        public int BuWizzOutputLevel { get; set; }
        public int BuWizz2OutputLevel { get; set; }

        public IEnumerable<DeviceOutputViewModel> DeviceOutputs { get; }

        public override async void OnAppearing()
        {
            _isDisappearing = false;
            base.OnAppearing();

            if (Device.DeviceType != DeviceType.Infrared)
            {
                if (!await _deviceManager.IsBluetoothOnAsync())
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("TurnOnBluetoothToConnect"),
                        Translate("Ok"),
                        DisappearingToken);

                    await NavigationService.NavigateBackAsync();
                    return;
                }
            }

            _connectionTokenSource = new CancellationTokenSource();
            _connectionTask = ConnectAsync();
        }

        public override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (_connectionTokenSource is not null && _connectionTask is not null)
            {
                _connectionTokenSource?.Cancel();
                await _connectionTask;
            }

            await Device.DisconnectAsync();
        }

        private async Task RenameDeviceAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Device.Name,
                    Translate("DeviceName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (deviceName) => !string.IsNullOrEmpty(deviceName),
                    DisappearingToken);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("DeviceNameCanNotBeEmpty"),
                            Translate("Ok"),
                            DisappearingToken);

                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await Device.RenameDeviceAsync(Device, result.Result),
                        Translate("Renaming"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ConnectAsync()
        {
            while (!(_connectionTokenSource?.IsCancellationRequested ?? false))
            {
                if (Device.DeviceState != DeviceState.Connected)
                {
                    var connectionResult = DeviceConnectionResult.Ok;

                    var dialogResult = await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            using (token.Register(() => _connectionTokenSource?.Cancel()))
                            {
                                connectionResult = await Device.ConnectAsync(
                                    _reconnect,
                                    OnDeviceDisconnected,
                                    Enumerable.Empty<ChannelConfiguration>(),
                                    true,
                                    true,
                                    _connectionTokenSource?.Token ?? default);
                            }
                        },
                        Translate("ConnectingTo"),
                        Device.Name,
                        Translate("Cancel"),
                        _connectionTokenSource?.Token ?? default);

                    if (dialogResult.IsCancelled)
                    {
                        await Device.DisconnectAsync();

                        if (!_isDisappearing)
                        {
                            await NavigationService.NavigateBackAsync();
                        }

                        return;
                    }
                    else
                    {
                        if (connectionResult == DeviceConnectionResult.Error)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Warning"),
                                Translate("FailedToConnect"),
                                Translate("Ok"),
                                DisappearingToken);

                            if (!_isDisappearing)
                            {
                                await NavigationService.NavigateBackAsync();
                            }

                            return;
                        }
                        else
                        {
                            if (Device.DeviceType == DeviceType.BuWizz)
                            {
                                SetBuWizzOutputLevel(BuWizzOutputLevel);
                            }
                            else if (Device.DeviceType == DeviceType.BuWizz2)
                            {
                                SetBuWizzOutputLevel(BuWizz2OutputLevel);
                            }
                            // update command enablement
                            UpdateCommandsAvailability();
                        }
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private async Task ActivateShelfModeCommandAsync()
        {
            if (await _dialogService.ShowQuestionDialogAsync(
                Translate("ActivateShelfMode"),
                Translate("ActivateShelfModeConfirm"),
                Translate("Yes"),
                Translate("No"),
                DisappearingToken))
            {
                try
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            // send command and later cancel connection
                            await Device.ActiveShelfModeAsync();
                            _connectionTokenSource?.Cancel();
                            // disconnection is expected to be triggered by Back
                            await Task.Delay(500, DisappearingToken);
                            await NavigationService.NavigateBackAsync();
                        },
                        Translate("Applying"),
                        token: DisappearingToken);
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ActivateShelfModeFailed", ex),
                        Translate("Ok"),
                        DisappearingToken);
                }
            }
        }

        private async Task ScanAsync()
        {
            if (!await _deviceManager.IsBluetoothOnAsync())
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
                                scanTask = _deviceManager.ScanAsync(cts.Token);

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

                            if (scanTask != null)
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

        private void OnDeviceDisconnected(Device device)
        {
            // update command enablement
            UpdateCommandsAvailability();
        }

        private void UpdateCommandsAvailability()
        {
            ScanCommand.RaiseCanExecuteChanged();
            ActivateShelfModeCommand.RaiseCanExecuteChanged();
            OpenDeviceSettingsPageCommand.RaiseCanExecuteChanged();
            // to ensure that servo/stepper commands are enabled / disabled properly
            RaisePropertyChanged(nameof(IsServoOrStepperSupported));
        }

        private void SetBuWizzOutputLevel(int level)
        {
            Device.SetOutputLevel(level);
        }

        public class DeviceOutputViewModel : NotifyPropertyChangedSource
        {
            private readonly INavigationService _navigationService;
            private int _output;

            public DeviceOutputViewModel(INavigationService navigationService, Device device, int channel)
            {
                _navigationService= navigationService;
                Device = device;
                Channel = channel;
                Output = 0;

                TouchUpCommand = new Command(() => Output = 0);
                TestServoStepperCommand = new SafeCommand(OpenChannelSetupAsync, () => IsServoOrStepperSupported);
            }

            public Device Device { get; }
            public int Channel { get; }

            public int MinValue => -100;
            public int MaxValue => 100;

            public int Output
            {
                get { return _output; }
                set
                {
                    _output = value;
                    Device.SetOutput(Channel, (float)value / MaxValue);
                    RaisePropertyChanged();
                }
            }

            public bool IsServoOrStepperSupported =>
                Device.IsOutputTypeSupported(Channel, ChannelOutputType.ServoMotor) ||
                Device.IsOutputTypeSupported(Channel, ChannelOutputType.StepperMotor);

            public ICommand TouchUpCommand { get; }
            public ICommand TestServoStepperCommand { get; }

            private async Task OpenChannelSetupAsync()
            {
                var action = new ControllerAction
                {
                    DeviceId = Device.Id,
                    Channel = Channel,
                    MaxServoAngle = DEFAULT_MAX_SERVO_ANGLE,
                    ServoBaseAngle = DEFAULT_SERVO_BASE_ANGLE,
                    StepperAngle = DEFAULT_STEPPER_ANGLE,
                    // choose first supported output type
                    ChannelOutputType = Device.IsOutputTypeSupported(Channel, ChannelOutputType.ServoMotor) 
                        ? ChannelOutputType.ServoMotor
                        : ChannelOutputType.StepperMotor,
                };
                await _navigationService.NavigateToAsync<ChannelSetupPageViewModel>(new NavigationParameters(("device", Device),
                    ("controlleraction", action),
                    ("ischanneltest", true)));
            }
        }
    }
}
