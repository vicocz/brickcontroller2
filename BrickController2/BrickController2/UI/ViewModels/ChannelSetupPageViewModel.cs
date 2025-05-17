using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class ChannelSetupPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private bool _startOutputProcessing;
        private ChannelConfiguration _channelConfig;
        private CancellationTokenSource? _connectionTokenSource;
        private Task? _connectionTask;
        private bool _isDisappearing = false;

        private int _maxServoAngle;
        private int _servoBaseAngle;
        private int _stepperAngle;

        public ChannelSetupPageViewModel(
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
            Action = parameters.Get<ControllerAction>("controlleraction");
            MaxServoAngle = Action.ChannelOutputType == ChannelOutputType.ServoMotor ? Action.MaxServoAngle : 0;
            ServoBaseAngle = Action.ChannelOutputType == ChannelOutputType.ServoMotor ? Action.ServoBaseAngle : 0;
            StepperAngle = Action.ChannelOutputType == ChannelOutputType.StepperMotor ? Action.StepperAngle : 0;

            // setup channel config for testing
            UpdateChannelConfig();
            _startOutputProcessing = Action.ChannelOutputType == ChannelOutputType.StepperMotor;

            SaveChannelSettingsCommand = new SafeCommand(async () => await SaveChannelSettingsAsync(), () => !_dialogService.IsDialogOpen);
            AutoCalibrateServoCommand = new SafeCommand(async () => await AutoCalibrateServoAsync(), () => Device.CanAutoCalibrateOutput(Action.Channel));
            ResetServoBaseCommand = new SafeCommand(async () => await ResetServoBaseAngleAsync(), () => CanResetChannelOutput);
            StepperTestCommand = new SafeCommand<string>(value => TestChannelAsync(value, reset: true));
        }

        public Device Device { get; }
        public ControllerAction Action { get; }

        public bool IsServoChannelOutputType => Action.ChannelOutputType == ChannelOutputType.ServoMotor;
        public bool IsStepperChannelOutputType => Action.ChannelOutputType == ChannelOutputType.StepperMotor;
        public bool CanResetChannelOutput => Device.CanResetOutput(Action.Channel);

        public int ServoBaseAngle
        {
            get { return _servoBaseAngle; }
            set { _servoBaseAngle = value; RaisePropertyChanged(); }
        }
        public int MaxServoAngle
        {
            get { return _maxServoAngle; }
            set { _maxServoAngle = value; RaisePropertyChanged(); }
        }
        public int StepperAngle
        {
            get { return _stepperAngle; }
            set { _stepperAngle = value; RaisePropertyChanged(); }
        }

        public ICommand SaveChannelSettingsCommand { get; }
        public ICommand AutoCalibrateServoCommand { get; }
        public ICommand ResetServoBaseCommand { get; }
        public ICommand StepperTestCommand { get; }

        public override async void OnAppearing()
        {
            _isDisappearing = false;
            base.OnAppearing();

            if (Device.DeviceType != DeviceType.Infrared)
            {
                if (!_deviceManager.IsBluetoothOn)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("TurnOnBluetoothToConnect"),
                        Translate("Ok"),
                        DisappearingToken);

                    if (!_isDisappearing)
                    {
                        await NavigationService.NavigateBackAsync();
                    }
                    return;
                }
            }

            _connectionTokenSource = new CancellationTokenSource();
            _connectionTask = ConnectAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;
            base.OnDisappearing();

            if (_connectionTokenSource is not null && _connectionTask is not null)
            {
                _connectionTokenSource.Cancel();
                await _connectionTask;
            }

            await Device.DisconnectAsync();
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
                                await Device.ConnectAsync(
                                    false,
                                    OnDeviceDisconnected,
                                    _startOutputProcessing ? [_channelConfig] : [],
                                    _startOutputProcessing,
                                    false,
                                    token);
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
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private void OnDeviceDisconnected(Device device)
        {
        }

        private async Task SaveChannelSettingsAsync()
        {
            if (Action.ChannelOutputType == ChannelOutputType.ServoMotor)
            {
                Action.MaxServoAngle = MaxServoAngle;
                Action.ServoBaseAngle = ServoBaseAngle;
            }
            else if (Action.ChannelOutputType == ChannelOutputType.StepperMotor)
            {
                // ServoBaseAngle is used for testing only
                Action.StepperAngle = StepperAngle;
            }
            await NavigationService.NavigateModalBackAsync();
        }

        private async Task AutoCalibrateServoAsync()
        {
            await EnforceDisabledChannelOutputProcessing();
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    var result = await Device.AutoCalibrateOutputAsync(Action.Channel, token);
                    if (result.Success)
                    {
                        ServoBaseAngle = (int)(result.BaseServoAngle * 180);
                    }
                },
                Translate("Calibrating"),
                null,
                Translate("Cancel"),
                DisappearingToken);
        }

        private async Task ResetServoBaseAngleAsync()
        {
            await EnforceDisabledChannelOutputProcessing();
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    await Device.ResetOutputAsync(Action.Channel, ServoBaseAngle / 180F, token);
                },
                Translate("Reseting"),
                null,
                Translate("Cancel"),
                DisappearingToken);
        }

        private void UpdateChannelConfig()
        {
            _channelConfig = new ChannelConfiguration
            {
                Channel = Action.Channel,
                ChannelOutputType = Action.ChannelOutputType,
                // current settings
                MaxServoAngle = Action.ChannelOutputType == ChannelOutputType.ServoMotor ? MaxServoAngle : 0,
                ServoBaseAngle = ServoBaseAngle, // for testing applied both servo and stepper
                StepperAngle = Action.ChannelOutputType == ChannelOutputType.StepperMotor ? StepperAngle : 0
            };
        }

        private async Task TestChannelAsync(string parameter, bool reset = true)
        {
            var value = Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
            // ensure stepper / servo settings are up-to-date and output processing is set
            if (MaxServoAngle != _channelConfig.MaxServoAngle ||
                ServoBaseAngle != _channelConfig.ServoBaseAngle ||
                StepperAngle != _channelConfig.StepperAngle ||
                !_startOutputProcessing)
            {
                // update prerequsities for servo/stepper testing
                UpdateChannelConfig();
                _startOutputProcessing = true;
                // force reconnection
                await ReconnectDeviceAsync();
            }
            // simulate triggering of servo/stepper button
            await TestButtonAsync(value, reset).ConfigureAwait(false);
        }

        private async Task EnforceDisabledChannelOutputProcessing()
        {
            // if stepper angle has changed, we need to reconnect to apply it
            if (_startOutputProcessing)
            {
                // update prerequsities for servo/stepper calibration/reset
                _startOutputProcessing = false;
                // force reconnection
                await ReconnectDeviceAsync();
            }
        }

        private async Task ReconnectDeviceAsync()
        {
            // disconnect 
            await Device.DisconnectAsync();

            // await reconnection
            while (!DisappearingToken.IsCancellationRequested && Device.DeviceState != DeviceState.Connected)
            {
                await Task.Delay(100, DisappearingToken);
            }
        }

        private async Task TestButtonAsync(float value, bool reset = false)
        {
            // simulate triggering of button
            Device.SetOutput(Action.Channel, value);
            await Task.Delay(500, DisappearingToken);
            if (reset)
            {
                Device.SetOutput(Action.Channel, GameControllers.BUTTON_RELEASED);
            }
        }
    }
}
