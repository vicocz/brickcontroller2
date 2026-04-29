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
    internal class TechnicMoveDevice : WirelessProtocolBasedDevice
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

        private readonly OutputValuesGroup<Half> _outputValues = new(9);
        private readonly OutputValuesGroup<Half> _playVmValues = new(2);

        private bool _applyPlayVmMode;
        private int _calibratedZeroAngle; // zero ABS angle for steering C channel in non PLAYVM mode

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
        public override bool CanResetOutput(int channel) => channel == CHANNEL_C;

        public override bool CanChangeMaxServoAngle(int channel) => !EnablePlayVmMode && channel == CHANNEL_C;

        public override bool IsOutputTypeSupported(int channel, ChannelOutputType outputType)
            => outputType switch
            {
                // motor if not PLAYVM for all channels, if PLAYVM only for other channels than C channel
                ChannelOutputType.NormalMotor => !EnablePlayVmMode || channel != CHANNEL_C,
                // servo for both PLAYVM and normal mode but C channel only
                ChannelOutputType.ServoMotor => channel == CHANNEL_C,
                // other types (such as stepper) are not supported at all
                _ => false,
            };

        public override Task<DeviceConnectionResult> ConnectAsync(bool reconnect, Action<Device> onDeviceDisconnected, IEnumerable<ChannelConfiguration> channelConfigurations, bool startOutputProcessing, bool requestDeviceInformation, CancellationToken token)
        {
            // autodetect PLAYVM mode for A / B channels (as testing page should not be affected)
            _applyPlayVmMode = startOutputProcessing && EnablePlayVmMode &&
                channelConfigurations.Any(c => c.Channel == CHANNEL_VM || (c.Channel == CHANNEL_C && c.ChannelOutputType == ChannelOutputType.ServoMotor));

            return base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
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
                >= CHANNEL_1 and <= CHANNEL_6 => _outputValues.SetOutput(channel, Half.Abs(rawValue)),
                // rest of ports: such as A, B or C when not in PLAYVM mode - use value as is
                _ => _outputValues.SetOutput(CheckChannel(channel), rawValue)
            };
        }

        public override async Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(300, token);
            await ResetServoAsync(channel, Convert.ToInt32(value * 180), token);
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
            return channelIndex >= 0;
        }

        protected override async ValueTask BeforeDisconnectAsync(CancellationToken token)
        {
            await base.BeforeDisconnectAsync(token);

            if (_applyPlayVmMode)
            {
                // reset hub LED
                var ledCmd = BuildPortOutput_DirectMode(PORT_HUB_LED, HUB_LED_MODE_COLOR, HUB_LED_COLOR_WHITE);
                await WriteAsync(ledCmd, token: token);
                await DelayAsync(token);
            }
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            // Wait until ports finish communicating with the hub
            await Task.Delay(1000, token); //TODO

            if (await base.AfterConnectSetupAsync(requestDeviceInformation, token))
            {
                try
                {
                    if (requestDeviceInformation)
                    {
                        await RequestHubPropertiesAsync(token);
                    }

                    // hub LED
                    var color = _applyPlayVmMode ? HUB_LED_COLOR_MAGENTA : HUB_LED_COLOR_WHITE;
                    var ledCmd = BuildPortOutput_DirectMode(PORT_HUB_LED, HUB_LED_MODE_COLOR, color);
                    await WriteAsync(ledCmd, token: token);
                    await DelayAsync(token);

                    // switch lights off
                    var lightsOffCmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, PORT_6LEDS_ALL_LIGHTS, 0x00);
                    var result = await WriteAsync(lightsOffCmd, token: token);
                    await DelayAsync(token);

                    // port configuration
                    for (int channel = 0; channel < NumberOfChannels; channel++)
                    {
                        var channelConfig = ChannelConfigs[channel];
                        if (channelConfig.OutputType == ChannelOutputType.ServoMotor)
                        {
                            await SetupChannelForPortInformationAsync(channel, token);
                            await Task.Delay(300, token);
                            await ResetServoAsync(channel, channelConfig.ServoBaseAngle, token);
                        }
                    }

                    return result;
                }
                catch
                {
                }
            }

            return false;
        }

        protected override void ResetOutputValues()
        {
            if (_applyPlayVmMode)
            {
                // output values - clear always lights — suppress initial burst to avoid flooding the hub
                _outputValues.Clear();
            }
            else
            {
                // otherwise all channels to be initialized
                _outputValues.Initialize();
            }
            _playVmValues.Clear();
            _calibratedZeroAngle = default;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
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

        private async Task<bool> SetupChannelForPortInformationAsync(int channel, CancellationToken token)
        {
            try
            {
                var portId = GetPortId(channel);

                if (_applyPlayVmMode)
                {
                    // setup channel to report APOS position
                    var inputFormatForAbsAngle = BuildPortInputFormatSetup(portId, PORT_MODE_3);
                    return await WriteAsync(inputFormatForAbsAngle, token);
                }

                // setup channel to for APOS, but no notifications
                var inputFormatForAbsAngleDisabled = BuildPortInputFormatSetup(portId, PORT_MODE_3, notification: PORT_VALUE_NOTIFICATION_DISABLED);
                await WriteAsync(inputFormatForAbsAngleDisabled, token);
                await Task.Delay(50, token);

                // query current APOS
                await WriteAsync([0x05, 0x00, 0x21, portId, 0x00], token);
                await Task.Delay(250, token); //TODO wait for change

                // setup channel to report POS position regularly
                var inputFormatForRelAngle = BuildPortInputFormatSetup(portId, PORT_MODE_2);
                await WriteAsync(inputFormatForRelAngle, token);
                await Task.Delay(250, token); //TODO wait for change

                // need to recalculate zero angle to support ABS POS commands
                _calibratedZeroAngle = CalculateCalibratedTarget(channel);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
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

                    await AwaitStableAbsolutePositionAsync(channel, TimeSpan.FromSeconds(4), token);
                }
                else
                {
                    // use simple Goto ABS position
                    var portId = GetPortId(channel);
                    var servoCmd = BuildPortOutput_GotoAbsPosition(portId, _calibratedZeroAngle + baseAngle, servoSpeed: 0x28);
                    await WriteAsync(servoCmd, token: token);

                    await AwaitStableAbsolutePositionAsync(channel, TimeSpan.FromSeconds(1), token);
                }

                return true;
            }
            catch
            {
                return false;
            }
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
                        var channelOutputType = GetOutputType(change.Key);

                        result = change.Key switch
                        {
                            // Light channels 1 - 6 require absolute value
                            >= CHANNEL_1 and <= CHANNEL_6 => await SendPortOutput_6LedAsync(ledIndex: change.Key - CHANNEL_1, value, token),
                            // all channels command - use original value
                            int.MaxValue => await SendAllOutputValuesAsync(change.Value, token),
                            // classic output command for A, B, C channels (with servo support)
                            CHANNEL_C when channelOutputType == ChannelOutputType.ServoMotor => await SendServoValue(change.Key, value, token),
                            _ => await SendPortOutput_ValueAsync(change.Key, value, token),
                        };

                        if (!result)
                        {
                            return false;
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
                    var maxServoAngle = GetMaxServoAngle(CHANNEL_C);
                    var speed = ToByte(values[PLAYVM_CHANNEL_DRIVE]);
                    var servoValue = maxServoAngle * (int)values[PLAYVM_CHANNEL_STEER] / 100;
                    var playVmCmd = BuildPortOutput_PlayVm(speed, servoValue);

                    if (!await WriteAsync(playVmCmd, token))
                    {
                        await DelayAsync(token);
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

        private Task<bool> SendPortOutput_6LedAsync(int ledIndex, byte value, CancellationToken token)
            => SendPortOutput_6LedMaskAsync(ToByte(1 << ledIndex), value, token);

        private async Task<bool> SendPortOutput_6LedMaskAsync(byte lightMask, byte value, CancellationToken token)
        {
            var cmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, lightMask, value);
            return await WriteAsync(cmd, token);
        }

        private async Task<bool> SendPortOutput_ValueAsync(int channel, byte value, CancellationToken token)
        {
            byte[] cmd = [8, 0x00, 0x81, GetPortId(channel), 0x11, 0x51, 0x00, value];
            return await WriteAsync(cmd, token);
        }

        private async Task<bool> SendServoValue(int channel, int value, CancellationToken token)
        {
            var portId = GetPortId(channel);
            // in non PLAYVM mode, need to apply calibrated base angle as offset to reach correct position
            var absPosition = _calibratedZeroAngle + ChannelConfigs[channel].ServoBaseAngle + value; //TODO MAX SERVO ANGLE
            var cmd = BuildPortOutput_GotoAbsPosition(portId, absPosition, servoSpeed: 50);
            return await WriteAsync(cmd, token);
        }

        private async Task<bool> SendAllOutputValuesAsync(Half value, CancellationToken token)
        {
            var rawValue = ToByte(value);
            // all LEDs at once
            var result = await SendPortOutput_6LedMaskAsync(PORT_6LEDS_ALL_LIGHTS, rawValue, token);

            // A, B, C channels
            foreach (var channel in new[] { CHANNEL_A, CHANNEL_B, CHANNEL_C })
            {
                var outputType = ChannelConfigs[channel].OutputType;
                result = result && outputType switch
                {
                    ChannelOutputType.ServoMotor => await SendServoValue(channel, (int)value, token),
                    _ => await SendPortOutput_ValueAsync(channel, rawValue, token),
                };
            }

            return result;
        }
    }
}
