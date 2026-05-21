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

using static BrickController2.Diagnostics.Logs;
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
            try
            {
                // wait until ports finish communicating with the hub
                await AwaitPeripheralsAttachedAsync(TimeSpan.FromSeconds(1), token);

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
                var lightOffCmd = BuildPortOutput_PlayVm(vmCmd: PLAYVM_LIGHTS_OFF_OFF);
                await WriteAsync(lightOffCmd, token: token);
                var result = await SendPortOutput_6LedMaskAsync(PORT_6LEDS_ALL_LIGHTS, 0x00, token);
                await DelayAsync(token);

                // port configuration
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    var channelConfig = ChannelConfigs.Get(channel);
                    if (channelConfig.OutputType == ChannelOutputType.ServoMotor)
                    {
                        await SetupChannelForPortInformationAsync(channel, token);
                        await ResetServoAsync(channel, channelConfig.ServoBaseAngle, token);
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        protected override void ResetOutputValues()
        {
            base.ResetOutputValues();
            if (_applyPlayVmMode)
            {
                _playVmValues.Initialize();
                // output values - clear always lights — suppress initial burst to avoid flooding the hub
                _outputValues.Clear();
            }
            else
            {
                _playVmValues.Clear();
                // otherwise all channels to be initialized
                _outputValues.Initialize();
            }
            _calibratedZeroAngle = default;
        }

        protected override async Task<bool> SendOutputValuesAsync(CancellationToken token)
        {
            try
            {
                // conditionally send PLAYVM command if PLAYVM mode is active
                var result = await SendPlayVmOutputValueAsync(token);

                // process changes for other channels as it's a light or a classic drive
                if (result)
                {
                    if (!_outputValues.TryGetChanges(out var changes))
                    {
                        return false;
                    }

                    foreach (KeyValuePair<int, Half> change in changes)
                    {
                        var value = ToByte(change.Value);

                        result = change.Key switch
                        {
                            // Light channels 1 - 6 require absolute value
                            >= CHANNEL_1 and <= CHANNEL_6 => await SendPortOutput_6LedAsync(ledIndex: change.Key - CHANNEL_1, value, token),
                            // all channels command - use original value
                            int.MaxValue => await SendAllOutputValuesAsync(change.Value, token),
                            // classic output command for A, B, C channels
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

        protected override bool TryProcessMessageData(byte messageType, ReadOnlySpan<byte> data)
        {
            switch (messageType)
            {
                case MESSAGE_TYPE_OUTPUT_COMMAND_FEEDBACK: // Port output command feedback
                    // PORT_PLAYVM completion feedback (0x82) signals calibration finished
                    if (data.Length >= 5 && data[3] == PORT_PLAYVM && (data[4] & 0x02) != 0)
                    {
                        Dump("Output command feedback", data);
                        _playVmCalibrationTcs?.TrySetResult(true);
                        return true;
                    }
                    break;
            }
            return base.TryProcessMessageData(messageType, data);
        }

        private async ValueTask<bool> SetupChannelForPortInformationAsync(int channel, CancellationToken token)
        {
            try
            {
                var portId = GetPortId(channel);

                if (_applyPlayVmMode)
                {
                    // setup channel to report Mode 3: GOPOS (Absolute Position)
                    var inputFormatForAbsAngle = BuildPortInputFormatSetup(portId, PORT_MODE_3);
                    await WriteAsync(inputFormatForAbsAngle, token);
                    await Task.Delay(300, token);
                    return true;
                }

                // setup channel to for Mode 3: GOPOS (Absolute Position), but no notifications
                var inputFormatForAbsAngleDisabled = BuildPortInputFormatSetup(portId, PORT_MODE_3, notification: PORT_VALUE_NOTIFICATION_DISABLED);
                await WriteAsync(inputFormatForAbsAngleDisabled, token);
                await Task.Delay(50, token);

                // query current GOPOS
                ChannelAbsPositions.ConsumeUpdate(channel); // clear existing value
                await WriteAsync([0x05, 0x00, MESSAGE_TYPE_PORT_INFORMATION_REQUEST, portId, 0x00], token);
                await AwaitPositionChangeAsync(() => ChannelAbsPositions.Get(channel),
                    TimeSpan.FromMilliseconds(250), token);

                // setup channel to report POS position regularly
                ChannelRelativePositions.ConsumeUpdate(channel); // clear existing value
                var inputFormatForRelAngle = BuildPortInputFormatSetup(portId, PORT_MODE_2);
                await WriteAsync(inputFormatForRelAngle, token);
                await AwaitPositionChangeAsync(() => ChannelRelativePositions.Get(channel),
                    TimeSpan.FromMilliseconds(250), token);

                // need to recalculate zero angle to support ABS POS commands
                _calibratedZeroAngle = CalculateCalibratedTarget(channel);

                return true;

                int CalculateCalibratedTarget(int channel, int targetBaseAngle = 0)
                {
                    int currentGopos = GetAbsPosition(channel);           // True physical angle (e.g., 90)
                    var position = ChannelRelativePositions.Get(channel); // Accumulated hub angle (e.g., 1080)

                    // Calculate the shortest physical distance to your target
                    // We use GOPOS here because it represents the actual hardware marker
                    int diffToTarget = NormalizeAngle(targetBaseAngle - currentGopos);

                    // Apply that physical difference to the Hub's accumulated POS
                    // If POS is 1080 and we need to move -90 degrees, the target is 990.
                    int targetHubPos = position.Current + diffToTarget;

                    return targetHubPos;
                }
            }
            catch
            {
                return false;
            }
        }

        private async ValueTask<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
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

                    // set up completion waiter before sending calibrate to avoid the race where
                    // feedback arrives before we start waiting
                    _playVmCalibrationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                    // do calibration
                    var calibrateCmd = BuildPortOutput_PlayVm(servoValue: baseAngle, vmCmd: PLAYVM_CALIBRATE_STEERING);
                    await WriteAsync(calibrateCmd, token: token);

                    // wait for the hub's completion feedback
                    try
                    {
                        await _playVmCalibrationTcs.Task.WaitAsync(TimeSpan.FromSeconds(4), token);
                        Dump("Command feedback: PLAYVM", ChannelAbsPositions.Get(channel));
                    }
                    catch (TimeoutException)
                    {
                        // hub did not respond in time, fall back to a short safety delay
                        await Task.Delay(500, token);
                    }
                    finally
                    {
                        _playVmCalibrationTcs = null;
                    }
                    // Wait for position to stabilize before allowing the output loop to start
                    await AwaitStablePositionAsync(() => ChannelAbsPositions.Get(channel), TimeSpan.FromSeconds(4), token);
                    Dump("Reset Servo", ChannelAbsPositions.Get(channel));
                }
                else
                {
                    // use simple Goto ABS position
                    var portId = GetPortId(channel);
                    var servoCmd = BuildPortOutput_GotoAbsPosition(portId, _calibratedZeroAngle + baseAngle, servoSpeed: 0x28);
                    await WriteAsync(servoCmd, token: token);

                    // Wait for position to stabilize before allowing the output loop to start
                    await AwaitStablePositionAsync(() => ChannelRelativePositions.Get(channel), TimeSpan.FromSeconds(2), token);
                    Dump("Reset Servo", ChannelRelativePositions.Get(channel));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async ValueTask<bool> SendPlayVmOutputValueAsync(CancellationToken token)
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

        private ValueTask<bool> SendPortOutput_6LedAsync(int ledIndex, byte value, CancellationToken token)
            => SendPortOutput_6LedMaskAsync(ToByte(1 << ledIndex), value, token);

        private ValueTask<bool> SendPortOutput_6LedMaskAsync(byte lightMask, byte value, CancellationToken token)
        {
            var cmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, lightMask, value);
            return WriteAsync(cmd, token);
        }

        private ValueTask<bool> SendPortOutput_ValueAsync(int channel, byte value, CancellationToken token)
        {
            byte[] cmd = [8, 0x00, PORT_OUTPUT_COMMAND, GetPortId(channel), FEEDBACK_ACTION_BOTH, PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, 0x00, value];
            return WriteAsync(cmd, token);
        }

        private async ValueTask<bool> SendAllOutputValuesAsync(Half value, CancellationToken token)
        {
            var rawValue = ToByte(value);
            // all LEDs at once
            var result = await SendPortOutput_6LedMaskAsync(PORT_6LEDS_ALL_LIGHTS, rawValue, token);

            // A, B, C channels
            foreach (var channel in new[] { CHANNEL_A, CHANNEL_B, CHANNEL_C })
            {
                var outputType = GetOutputType(channel);
                result = result && outputType switch
                {
                    _ => await SendPortOutput_ValueAsync(channel, rawValue, token),
                };
            }

            return result;
        }
    }
}
