using BrickController2.CreationManagement;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement
{
    internal class TechnicMoveDevice : ControlPlusDevice
    {
        public const int CHANNEL_VM = 12; // artificial channel to mimic combined AB ports in PLAYVM

        private const int CHANNEL_C = 2;
        private const string EnablePlayVmSettingName = "PlayVmEnabled";

        private bool _applyPlayVmMode;
        private volatile byte _virtualMotorValue;

        public TechnicMoveDevice(string name,
            string address,
            IEnumerable<NamedSetting> settings,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            // apply value (if any) or TRUE by default
            SetSettingValue(EnablePlayVmSettingName, settings, true);
        }

        public override DeviceType DeviceType => DeviceType.TechnicMove;
        public override int NumberOfChannels => 9;

        public bool EnablePlayVmMode => GetSettingValue(EnablePlayVmSettingName, true);

        public override bool CanAutoCalibrateOutput(int channel) => false;
        public override bool CanResetOutput(int channel) => EnablePlayVmMode && channel == CHANNEL_C;

        public override bool CanChangeMaxServoAngle(int channel) => false;

        public override bool IsOutputTypeSupported(int channel, ChannelOutputType outputType)
            => outputType switch
            {
                // motor if not PLAYVM for all channels, if PLAYVM only for other channels than C channel
                ChannelOutputType.NormalMotor => !EnablePlayVmMode || channel != CHANNEL_C,
                // servo only for PLAYVM and C channel
                ChannelOutputType.ServoMotor => EnablePlayVmMode && channel == CHANNEL_C,
                // other types (such as stepper) are not supported at all
                _ => false,
            };

        public override Task<DeviceConnectionResult> ConnectAsync(bool reconnect, Action<Device> onDeviceDisconnected, IEnumerable<ChannelConfiguration> channelConfigurations, bool startOutputProcessing, bool requestDeviceInformation, CancellationToken token)
        {
            // autodetect PLAYVM mode for A / B channels (as testing page should not be affected)
            _applyPlayVmMode = startOutputProcessing && EnablePlayVmMode &&
                channelConfigurations.Any(c => c.Channel == CHANNEL_VM || (c.Channel == CHANNEL_C && c.ChannelOutputType == ChannelOutputType.ServoMotor));

            // filter out non-standard channels and configurations with unsupported output types
            var filteredConfigurations = channelConfigurations
                .Where(c => c.Channel != CHANNEL_VM)
                .Where(c => IsOutputTypeSupported(c.Channel, c.ChannelOutputType))
                .ToArray();

            return base.ConnectAsync(reconnect, onDeviceDisconnected, filteredConfigurations, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            if (channel == CHANNEL_VM)
            {
                // reset servo writes to enforce update
                ResetSendAttemps(CHANNEL_C);
                // store virtual motor value to be later send with PLAYVM
                var intValue = (int)(100 * CutOutputValue(value));
                _virtualMotorValue = GetChannelValue(intValue);
            }
            else
            {
                base.SetOutput(channel, value);
            }
        }

        protected override byte GetPortId(int channelIndex) => channelIndex switch
        {
            0 => PORT_DRIVE_MOTOR_1,
            1 => PORT_DRIVE_MOTOR_2,
            2 => PORT_STEERING_MOTOR,
            3 or 4 or 5 or 6 or 7 or 8 => PORT_6LEDS,
            _ => throw new ArgumentException($"Value of channel '{channelIndex}' is out of supported range.", nameof(channelIndex))
        };

        protected override bool TryGetChannelIndex(byte portId, out int channelIndex)
        {
            channelIndex = portId switch
            {
                PORT_DRIVE_MOTOR_1 => 0,
                PORT_DRIVE_MOTOR_2 => 1,
                PORT_STEERING_MOTOR => 2,

                _ => -1
            };
            return channelIndex >= 0;
        }

        protected override byte GetChannelValue(int value) => ToByte(value);

        protected override void InitializeChannelInfo(int channel, int lastOutputValue = 1, int sendAttemptsLeft = 10)
        {
            // if PLAYVM enabled, reset A / B channels differently in order to avoid output writes
            if (_applyPlayVmMode && channel < CHANNEL_C)
            {
                lastOutputValue = 0;
                sendAttemptsLeft = 0;
            }
            else if (channel > CHANNEL_C)
            {
                // no need to update lights
                lastOutputValue = 0;
                sendAttemptsLeft = 0;
            }
            base.InitializeChannelInfo(channel, lastOutputValue, sendAttemptsLeft);
        }

        protected override byte[] GetOutputCommand(int channel, int value)
        {
            // 6LED
            var ledIndex = channel - 3;
            if (ledIndex >= 0)
            {
                var rawValue = ToByte(Math.Abs(value));
                var ledMask = ToByte(1 << ledIndex);
                return BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, ledMask, rawValue);
            }
            return base.GetOutputCommand(channel, value);
        }

        protected override byte[] GetServoCommand(int channel, int servoValue, int servoSpeed)
        {
            if (_applyPlayVmMode)
            {
                return BuildPortOutput_PlayVm(speedValue: _virtualMotorValue, servoValue: servoValue);
            }

            var portId = GetPortId(channel);
            return BuildPortOutput_GotoAbsPosition(portId, servoValue, (byte)servoSpeed);
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            if (await base.AfterConnectSetupAsync(requestDeviceInformation, token))
            {
                try
                {
                    // hub LED
                    var color = _applyPlayVmMode ? HUB_LED_COLOR_MAGENTA : HUB_LED_COLOR_WHITE;
                    var ledCmd = BuildPortOutput_DirectMode(PORT_HUB_LED, HUB_LED_MODE_COLOR, color);
                    await WriteAsync(ledCmd, token: token);
                    await Task.Delay(20, token);

                    // switch lights off
                    var lightsOffCmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, PORT_6LEDS_ALL_LIGHTS, 0x00);
                    var result = await WriteAsync(lightsOffCmd, token: token);
                    await Task.Delay(20, token);

                    return result;
                }
                catch
                {
                }
            }

            return false;
        }

        protected override async Task<bool> SetupChannelForPortInformationAsync(int channel, CancellationToken token)
        {
            try
            {
                // setup channel to report ABS position
                var portId = GetPortId(channel);
                var inputFormatForAbsAngle = BuildPortInputFormatSetup(portId, PORT_MODE_3);
                return await WriteAsync(inputFormatForAbsAngle, token);
            }
            catch
            {
                return false;
            }
        }

        protected override async Task<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
        {
            try
            {
                if (_applyPlayVmMode)
                {
                    // reset servo via PLAYVM
                    // PLAYVM cmd supports only servo on C channel
                    var servoCmd = BuildPortOutput_PlayVm(servoValue: baseAngle, vmCmd: PLAYVM_COMMAND);
                    await WriteAsync(servoCmd, token: token);
                    await Task.Delay(100, token);

                    // do calibration
                    var calibrateCmd = BuildPortOutput_PlayVm(servoValue: baseAngle, vmCmd: PLAYVM_CALIBRATE_STEERING);
                    await WriteAsync(calibrateCmd, token: token);
                }
                else
                {
                    // use simple Goto ABS position
                    var portId = GetPortId(channel);
                    var servoCmd = BuildPortOutput_GotoAbsPosition(portId, baseAngle, servoSpeed: 0x28);
                    await WriteAsync(servoCmd, token: token);
                }

                // need to wait till it completes
                await AwaitStableAbsPositionAsync(channel, TimeSpan.FromSeconds(4), token);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
