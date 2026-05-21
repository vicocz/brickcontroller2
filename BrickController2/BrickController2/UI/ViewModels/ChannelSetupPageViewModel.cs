using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using static BrickController2.CreationManagement.ControllerDefaults;

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
            IsChannelTest = parameters.Get("ischanneltest", false);
            MaxServoAngle = Action.MaxServoAngle;
            ServoBaseAngle = Action.ServoBaseAngle;
            StepperAngle = Action.StepperAngle;

            // setup channel config for testing
            UpdateChannelConfig();
            _startOutputProcessing = Action.ChannelOutputType == ChannelOutputType.StepperMotor;

            SaveChannelSettingsCommand = new SafeCommand(async () => await SaveChannelSettingsAsync(), () => !IsChannelTest && !_dialogService.IsDialogOpen);
            AutoCalibrateServoCommand = new SafeCommand(async () => await AutoCalibrateServoAsync(), () => Device.CanAutoCalibrateOutput(Action.Channel));
            ResetServoBaseCommand = new SafeCommand(async () => await ResetServoBaseAngleAsync(), () => CanResetChannelOutput);
            StepperTestCommand = new SafeCommand<string>(value => TestChannelAsync(value, reset: true));
            ServoTestCommand = new SafeCommand<string>(value => TestChannelAsync(value, reset: false));
            SelectChannelOutputTypeCommand = new SafeCommand(SelectChannelOutputTypeAsync, () => IsChannelTest);
            ResetMaxServoAngleCommand = new SafeCommand(() => MaxServoAngle = DEFAULT_MAX_SERVO_ANGLE, () => MaxServoAngle != DEFAULT_MAX_SERVO_ANGLE);
            ResetServoBaseAngleCommand = new SafeCommand(() => ServoBaseAngle = DEFAULT_SERVO_BASE_ANGLE, () => ServoBaseAngle != DEFAULT_SERVO_BASE_ANGLE && CanResetChannelOutput);
            ResetStepperAngleCommand = new SafeCommand(() => StepperAngle = DEFAULT_STEPPER_ANGLE, () => StepperAngle != DEFAULT_STEPPER_ANGLE);
        }

        public Device Device { get; }
        public ControllerAction Action { get; }
        public bool CanResetChannelOutput => Device.CanResetOutput(Action.Channel);

        public bool IsChannelTest { get; }

        public int ServoBaseAngle
        {
            get { return _servoBaseAngle; }
            set { _servoBaseAngle = value; RaisePropertyChanged(); ResetServoBaseAngleCommand.RaiseCanExecuteChanged(); }
        }
        public int MaxServoAngle
        {
            get { return _maxServoAngle; }
            set { _maxServoAngle = value; RaisePropertyChanged(); ResetMaxServoAngleCommand.RaiseCanExecuteChanged(); }
        }
        public int StepperAngle
        {
            get { return _stepperAngle; }
            set { _stepperAngle = value; RaisePropertyChanged(); ResetStepperAngleCommand.RaiseCanExecuteChanged(); }
        }

        public ICommand SaveChannelSettingsCommand { get; }
        public ICommand AutoCalibrateServoCommand { get; }
        public ICommand ResetServoBaseCommand { get; }
        public ICommand StepperTestCommand { get; }
        public ICommand ServoTestCommand { get; }
        public ICommand SelectChannelOutputTypeCommand { get; }
        public ICommand ResetStepperAngleCommand { get; }
        public ICommand ResetMaxServoAngleCommand { get; }
        public ICommand ResetServoBaseAngleCommand { get; }

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
            try
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
            catch (OperationCanceledException)
            {
                // ignore cancellation
            }
        }

        private async Task ResetServoBaseAngleAsync()
        {
            try
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
            catch (OperationCanceledException)
            {
                // ignore cancellation
            }
        }

        private void UpdateChannelConfig()
        {
            _channelConfig = new ChannelConfiguration
            {
                Channel = Action.Channel,
                ChannelOutputType = Action.ChannelOutputType,
                // current settings
                MaxServoAngle = MaxServoAngle,
                ServoBaseAngle = ServoBaseAngle, // for testing applied both servo and stepper
                StepperAngle = StepperAngle
            };
        }

        private async Task TestChannelAsync(string parameter, bool reset = true)
        {
            try
            {
                var value = Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
                // ensure stepper / servo settings are up-to-date and output processing is set
                if (MaxServoAngle != _channelConfig.MaxServoAngle ||
                    ServoBaseAngle != _channelConfig.ServoBaseAngle ||
                    StepperAngle != _channelConfig.StepperAngle ||
                    Action.ChannelOutputType != _channelConfig.ChannelOutputType ||
                    !_startOutputProcessing)
                {
                    // update prerequsities for servo/stepper testing
                    UpdateChannelConfig();
                    // force reconnection with processing enabled
                    await EnforceEnabledChannelOutputProcessing(true);
                }
                // simulate triggering of servo/stepper button
                await TestButtonAsync(value, reset).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // ignore cancellation
            }
        }

        private async Task EnforceEnabledChannelOutputProcessing(bool force = false)
        {
            if (!_startOutputProcessing|| force)
            {
                // update prerequisites for servo/stepper calibration/reset
                _startOutputProcessing = true;
                // force reconnection
                await ReconnectDeviceAsync();
            }
        }

        private async Task EnforceDisabledChannelOutputProcessing()
        {
            // if output processing was required, reset it now
            if (_startOutputProcessing)
            {
                // update prerequisites for servo/stepper calibration/reset
                _startOutputProcessing = false;
                // force reconnection
                await ReconnectDeviceAsync();
            }
        }

        private async Task ReconnectDeviceAsync()
        {
            // disconnect 
            await Device.DisconnectAsync();

            // await reconnection (and closing of dialog if any)
            while (!DisappearingToken.IsCancellationRequested &&
                (Device.DeviceState != DeviceState.Connected || _dialogService.IsDialogOpen))
            {
                await Task.Delay(50, DisappearingToken);
            }
        }

        private async Task TestButtonAsync(float value, bool reset = false)
        {
            // simulate triggering of button
            Device.SetOutput(Action.Channel, value);
            await Task.Delay(500, DisappearingToken);
            if (reset)
            {
                Device.SetOutput(Action.Channel, InputDevices.BUTTON_RELEASED);
            }
        }

        private async Task SelectChannelOutputTypeAsync()
        {
            try
            {
                // do filtering based on device capabilities
                var channelOutputTypes = new[] { ChannelOutputType.ServoMotor, ChannelOutputType.StepperMotor }
                    .Where(x => Device.IsOutputTypeSupported(Action.Channel, x))
                    .Select(x => Enum.GetName(x)!)
                    .ToArray();

                var result = await _dialogService.ShowSelectionDialogAsync(
                    channelOutputTypes,
                    Translate("ChannelType"),
                    Translate("Cancel"),
                    DisappearingToken);

                if (result.IsOk &&
                    Enum.TryParse<ChannelOutputType>(result.SelectedItem, out var newChannelType) &&
                    newChannelType != Action.ChannelOutputType)
                {
                    // update channel output type and trigger UI update
                    Action.ChannelOutputType = newChannelType;
                    // update prerequsities for servo/stepper testing
                    UpdateChannelConfig();

                    // by default enable output processing for testing servo/stepper motor
                    await EnforceEnabledChannelOutputProcessing(true);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore cancellation
            }
        }
    }
}
