using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.IO;
using BrickController2.DeviceManagement.Lego;
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
    internal class TechnicMoveDevice : ControlPlusDeviceBase
    {
        public const int CHANNEL_VM = 12; // artificial channel to mimic combined AB ports in PLAYVM

        private const int CHANNEL_A = 0;
        private const int CHANNEL_B = 1;
        private const int CHANNEL_C = 2;
        private const int CHANNEL_1 = 3; // Light #1
        private const int CHANNEL_6 = 8; // Light #6
        private const int PLAYVM_CHANNEL_DRIVE = 0;
        private const int PLAYVM_CHANNEL_STEER = 1;
        private const string EnablePlayVmSettingName = "PlayVmEnabled";

        private readonly OutputValuesGroup<Half> _outputValues = new(3);
        private readonly OutputValuesGroup<Half> _playVmValues = new(2);

        private int _maxServoAngle;
        private int _servoBaseAngle;
        private bool _applyPlayVmMode;
        private TaskCompletionSource<bool>? _playVmCalibrationTcs;

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

        protected override bool AutoConnectOnFirstConnect => true;

        public override bool CanResetOutput(int channel) => EnablePlayVmMode && channel == CHANNEL_C;

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
            _applyPlayVmMode = startOutputProcessing &&
                channelConfigurations.Any(c => c.Channel == CHANNEL_VM || (c.Channel == CHANNEL_C && c.ChannelOutputType == ChannelOutputType.ServoMotor));

            // filter out non-standard channels and configurations with unsupported output types
            var filteredConfigurations = channelConfigurations
                .Where(c => c.Channel != CHANNEL_VM)
                .Where(c => IsOutputTypeSupported(c.Channel, c.ChannelOutputType))
                .ToArray();

            // update servo config, if set
            var servoConfig = filteredConfigurations.FirstOrDefault(c => c.Channel == CHANNEL_C && c.ChannelOutputType == ChannelOutputType.ServoMotor);
            _maxServoAngle = servoConfig.MaxServoAngle;
            _servoBaseAngle = servoConfig.ServoBaseAngle;

            return base.ConnectAsync(reconnect, onDeviceDisconnected, filteredConfigurations, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            var rawValue = (Half)(100 * CutOutputValue(value));

            _ = channel switch
            {
                // store A+B virtual channel value for PLAYVM
                CHANNEL_VM => _playVmValues.SetOutput(PLAYVM_CHANNEL_DRIVE, rawValue),
                // store C channel value for PLAYVM
                CHANNEL_C when _applyPlayVmMode => _playVmValues.SetOutput(PLAYVM_CHANNEL_STEER, rawValue),
                // Light channels 1 - 6 require absolute value
                >= CHANNEL_1 and <= CHANNEL_6 => _outputValues.SetOutput(CheckChannel(channel), Half.Abs(rawValue)),

                _ => _outputValues.SetOutput(CheckChannel(channel), rawValue)
            };
        }

        public override async Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(token);
            await Task.Delay(300, token);
            await ResetServoAsync(Convert.ToInt32(value * 180), token);
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
                // all other ports (PORT_6LEDS, PORT_PLAYVM, PORT_HUB_LED, etc.) are not tracked
                _ => -1
            };
            return channelIndex != -1;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                //  if PLAYVM enabled, suppress A / B / C channels — all controlled via PLAYVM commands
                _playVmValues.Clear();
                // output values - clear always lights — suppress initial burst to avoid flooding the hub's BLE receive buffer
                _outputValues.Clear();
 
                if (!_applyPlayVmMode)
                {
                    // otherwise force A B C channels to be sent
                    _outputValues.Initialize(CHANNEL_A);
                    _outputValues.Initialize(CHANNEL_B);
                    _outputValues.Initialize(CHANNEL_C);
                }

                while (!token.IsCancellationRequested)
                {
                    if (!await SendOutputValuesAsync(token).ConfigureAwait(false))
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            catch { }
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                // hub LED — cosmetic only, failure does not abort connection
                var color = _applyPlayVmMode ? HUB_LED_COLOR_MAGENTA : HUB_LED_COLOR_GREEN;
                var ledCmd = BuildPortOutput_HubLed(PORT_HUB_LED, HUB_LED_MODE_COLOR, color);
                await WriteAsync(ledCmd, token);

                // switch lights off
                var lightsOffCmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, 0xff, 0x00);
                await WriteAsync(lightsOffCmd, token);

                // setup channels for port information and reset servos to base angles if in PLAYVM mode, failure of which does not abort connection as well
                if (_applyPlayVmMode)
                {
                    await SetupChannelForPortInformationAsync(token);
                    await Task.Delay(300, token);
                    await ResetServoAsync(_servoBaseAngle, token);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SetupChannelForPortInformationAsync(CancellationToken token)
        {
            try
            {
                // setup channel to report ABS position - port mode 3
                var inputFormatForAbsAngle = BuildPortInputFormatSetup(PORT_STEERING_MOTOR, PORT_MODE_3);
                return await WriteAsync(inputFormatForAbsAngle, token);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ResetServoAsync(int baseAngle, CancellationToken token)
        {
            if (!EnablePlayVmMode)
            {
                return false;
            }

            try
            {
                // reset servo via PLAYVM
                // PLAYVM cmd supports only servo on C channel
                var servoCmd = BuildPortOutput_PlayVm(servoValue: baseAngle, vmCmd: PLAYVM_COMMAND);
                await WriteAsync(servoCmd, token);
                await Task.Delay(100, token);

                // set up completion waiter before sending calibrate to avoid the race where
                // feedback arrives before we start waiting
                _playVmCalibrationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                // do calibration
                var calibrateCmd = BuildPortOutput_PlayVm(servoValue: baseAngle, vmCmd: PLAYVM_CALIBRATE_STEERING);
                await WriteAsync(calibrateCmd, token);

                // wait for the hub's completion feedback instead of a fixed delay
                try
                {
                    await _playVmCalibrationTcs.Task.WaitAsync(TimeSpan.FromSeconds(2), token);
                }
                catch (TimeoutException)
                {
                    // hub did not respond in time, fall back to a short safety delay
                    await Task.Delay(500, token);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _playVmCalibrationTcs = null;
            }
        }

        protected override void OnPortOutputCommandFeedback(ReadOnlySpan<byte> data)
        {
            // PORT_PLAYVM completion feedback (0x82) signals calibration finished
            if (data.Length >= 5 && data[3] == PORT_PLAYVM && (data[4] & 0x02) != 0)
            {
                _playVmCalibrationTcs?.TrySetResult(true);
            }
            base.OnPortOutputCommandFeedback(data);
        }

        private async Task<bool> SendOutputValuesAsync(CancellationToken token)
        {
            try
            {
                // conditionally send PLAYVM command if PLAYVM mode is active
                var result = await SendPlayVmOutputValueAsync(token);

                // process changes for other channels as it's a light or a classic drive
                if (result && _outputValues.TryGetChanges(out var changes))
                {
                    foreach (KeyValuePair<int, Half> change in changes)
                    {
                        var value = ToByte(change.Value);

                        // 6LED
                        var ledIndex = change.Key - CHANNEL_1;

                        var outputCmd = change.Key switch
                        {
                            // Light channels 1 - 6 require absolute value
                            >= CHANNEL_1 and <= CHANNEL_6 => BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, ToByte(1 << ledIndex), value),
                            _ => [8, 0x00, 0x81, GetPortId(change.Key), 0x11, 0x51, 0x00, value]
                        };

                        if (!await WriteNoResponseAsync(outputCmd, SEND_DELAY, token))
                        {
                            break;
                        }
                    }

                    _outputValues.Commit();
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendPlayVmOutputValueAsync(CancellationToken token)
        {
            try
            {
                if (_applyPlayVmMode && _playVmValues.TryGetValues(out var values))
                {
                    var speed = ToByte(values[PLAYVM_CHANNEL_DRIVE]);
                    var servoValue = _maxServoAngle * (int)values[PLAYVM_CHANNEL_STEER] / 100;
                    var playVmCmd = BuildPortOutput_PlayVm(speed, servoValue);

                    if (!await WriteAsync(playVmCmd, token))
                    {
                        await Task.Delay(SEND_DELAY, token);
                        return false;
                    }

                    // commit when successfully sent
                    _playVmValues.Commit();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
