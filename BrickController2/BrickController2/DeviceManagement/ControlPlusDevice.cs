using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.Lego;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement
{
    internal abstract class ControlPlusDevice : WirelessProtocolBasedDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        private static readonly TimeSpan SEND_DELAY = TimeSpan.FromMilliseconds(10);
        private static readonly TimeSpan POSITION_EXPIRATION = TimeSpan.FromMilliseconds(200);

        private readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        private readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        private readonly byte[] _stepperSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0b, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        private readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };

        private readonly int[] _outputValues;
        private readonly int[] _lastOutputValues;
        private readonly int[] _sendAttemptsLeft;
        private readonly object _outputLock = new object();

        private readonly Stopwatch _lastSent_NormalMotor = new Stopwatch();

        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            _outputValues = new int[NumberOfChannels];
            _lastOutputValues = new int[NumberOfChannels];
            _sendAttemptsLeft = new int[NumberOfChannels];
        }

        public override bool IsOutputTypeSupported(int channel, ChannelOutputType outputType)
            // support all output types on all channels
            => true;

        protected override bool AutoConnectOnFirstConnect => true;

        /// <summary>
        /// After this time period has elapsed since the last transmission, the output values ​​are sent again.
        /// A value of TimeSpan.MaxValue disables the resending.
        /// </summary>
        protected virtual TimeSpan ResendDelay_NormalMotor => TimeSpan.MaxValue;

        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            lock (_outputLock)
                {
                    for (int c = 0; c < NumberOfChannels; c++)
                    {
                        _outputValues[c] = 0;
                        _lastOutputValues[c] = 0;
                    }
                }

            return await base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(100 * value);

            lock (_outputLock)
            {
                if (_outputValues[channel] != intValue)
                {
                    _outputValues[channel] = intValue;
                    _sendAttemptsLeft[channel] = MAX_SEND_ATTEMPTS;
                }
            }
        }

        public override bool CanResetOutput(int channel) => true;

        public override async Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(300, token);
            await ResetServoAsync(channel, Convert.ToInt32(value * 180), token);
        }

        public override bool CanAutoCalibrateOutput(int channel) => true;

        public override bool CanChangeMaxServoAngle(int channel) => true;

        public override async Task<(bool Success, float BaseServoAngle)> AutoCalibrateOutputAsync(int channel, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(TimeSpan.FromMilliseconds(300), token);
            return await AutoCalibrateServoAsync(channel, token);
        }

        protected virtual byte GetChannelValue(int value)
            // calculate raw motor value
            => (byte)(value < 0 ? (255 + value) : value);

        protected void ResetSendAttemps(int channel, int attemps = MAX_SEND_ATTEMPTS)
        {
            lock (_outputLock)
            {
                // do it conditionally
                if (_sendAttemptsLeft[channel] != MAX_SEND_ATTEMPTS)
                {
                    _sendAttemptsLeft[channel] = attemps;
                }
            }
        }

        protected virtual byte[] GetOutputCommand(int channel, int value)
        {
            // send base motor value (-100 .. 100 %)
            _sendBuffer[3] = GetPortId(channel);
            _sendBuffer[7] = GetChannelValue(value);

            return _sendBuffer;
        }

        protected virtual byte[] GetServoCommand(int channel, int servoValue, int servoSpeed)
        {
            _servoSendBuffer[3] = GetPortId(channel);
            _servoSendBuffer[6] = (byte)(servoValue & 0xff);
            _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
            _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
            _servoSendBuffer[9] = (byte)((servoValue >> 24) & 0xff);
            _servoSendBuffer[10] = (byte)servoSpeed;

            return _servoSendBuffer;
        }

        protected override void ResetOutputValues()
        {
            base.ResetOutputValues();

            lock (_outputLock)
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    _outputValues[channel] = 0;
                    _lastOutputValues[channel] = 1;
                    _sendAttemptsLeft[channel] = MAX_SEND_ATTEMPTS;
                }
            }
            _lastSent_NormalMotor.Reset();
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                // Wait until ports finish communicating with the hub
                await AwaitPeripheralsAttachedAsync(TimeSpan.FromMilliseconds(1000), token);

                if (requestDeviceInformation)
                {
                    await RequestHubPropertiesAsync(token);
                }

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    var channelConfig = ChannelConfigs.Get(channel);
                    if (channelConfig.OutputType == ChannelOutputType.ServoMotor)
                    {
                        await SetupChannelForPortInformationAsync(channel, token);
                        await Task.Delay(300, token);
                        await ResetServoAsync(channel, channelConfig.ServoBaseAngle, token);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override async Task<bool> SendOutputValuesAsync(CancellationToken token)
        {
            try
            {
                var result = true;

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    var outputType = GetOutputType(channel);
                    switch (outputType)
                    {
                        case ChannelOutputType.NormalMotor:
                            result = result && await SendOutputValueAsync(channel, token);
                            break;

                        case ChannelOutputType.ServoMotor:
                            result = result && await SendServoOutputValueAsync(channel, token);
                            break;

                        case ChannelOutputType.StepperMotor:
                            result = result && await SendStepperOutputValueAsync(channel, token);
                            break;
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueAsync(int channel, CancellationToken token)
        {
            try
            {
                int v, sendAttemptsLeft;

                lock (_outputLock)
                {
                    v = _outputValues[channel];
                    sendAttemptsLeft = _sendAttemptsLeft[channel];
                    _sendAttemptsLeft[channel] = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                }

                if (v != _lastOutputValues[channel] || 
                    sendAttemptsLeft > 0 ||
                    _lastSent_NormalMotor.Elapsed > ResendDelay_NormalMotor)
                {
                    var outputCmd = GetOutputCommand(channel, v);
                    if (await WriteAsync(outputCmd, token))
                    {
                        _lastSent_NormalMotor.Restart();

                        _lastOutputValues[channel] = v;
                        ResetSendAttemps(channel, 0);
                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueVirtualAsync(int virtualChannel, int channel1, int channel2, int value1, int value2, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel1] != value1 || _lastOutputValues[channel2] != value2)
                {
                    _virtualPortSendBuffer[3] = (byte)virtualChannel;
                    _virtualPortSendBuffer[6] = (byte)(value1 < 0 ? (255 + value1) : value1);
                    _virtualPortSendBuffer[7] = (byte)(value2 < 0 ? (255 + value2) : value2);

                    if (await WriteNoResponseAsync(_virtualPortSendBuffer, token))
                    {
                        _lastOutputValues[channel1] = value1;
                        _lastOutputValues[channel2] = value2;

                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendServoOutputValueAsync(int channel, CancellationToken token)
        {
            try
            {
                int v, sendAttemptsLeft;

                lock (_outputLock)
                {
                    v = _outputValues[channel];
                    sendAttemptsLeft = _sendAttemptsLeft[channel];
                    _sendAttemptsLeft[channel] = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                }

                if (v != _lastOutputValues[channel] || sendAttemptsLeft > 0)
                {
                    var servoValue = GetMaxServoAngle(channel) * v / 100;
                    var servoSpeed = CalculateServoSpeed(channel, servoValue);

                    if (servoSpeed == 0)
                    {
                        return true;
                    }

                    var servoCmd = GetServoCommand(channel, servoValue, servoSpeed);
                    if (await WriteAsync(servoCmd, token))
                    {
                        _lastOutputValues[channel] = v;
                        ResetSendAttemps(channel, 0);
                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendStepperOutputValueAsync(int channel, CancellationToken token)
        {
            try
            {
                int v, sendAttemptsLeft;

                lock (_outputLock)
                {
                    v = _outputValues[channel];
                    sendAttemptsLeft = _sendAttemptsLeft[channel];
                    _sendAttemptsLeft[channel] = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                }

                var stepperAngle = ChannelConfigs.Get(channel).StepperAngle;
                _stepperSendBuffer[3] = GetPortId(channel);
                _stepperSendBuffer[6] = (byte)(stepperAngle & 0xff);
                _stepperSendBuffer[7] = (byte)((stepperAngle >> 8) & 0xff);
                _stepperSendBuffer[8] = (byte)((stepperAngle >> 16) & 0xff);
                _stepperSendBuffer[9] = (byte)((stepperAngle >> 24) & 0xff);
                _stepperSendBuffer[10] = (byte)(v > 0 ? 50 : -50);

                if (v != _lastOutputValues[channel] && Math.Abs(v) == 100)
                {
                    if (await WriteAsync(_stepperSendBuffer, token))
                    {
                        _lastOutputValues[channel] = v;
                        ResetSendAttemps(channel, 0);
                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _lastOutputValues[channel] = v;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected virtual async Task<bool> SetupChannelForPortInformationAsync(int channel, CancellationToken token)
        {
            try
            {
                var portId = GetPortId(channel);
                var lockBuffer = new byte[] { 0x05, 0x00, 0x42, portId, 0x02 };
                var inputFormatForAbsAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, portId, 0x03, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var inputFormatForRelAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, portId, 0x02, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var modeAndDataSetBuffer = new byte[] { 0x08, 0x00, 0x42, portId, 0x01, 0x00, 0x30, 0x20 };
                var unlockAndEnableBuffer = new byte[] { 0x05, 0x00, 0x42, portId, 0x03 };

                var result = true;
                result = result && await WriteAsync(lockBuffer, token);
                await Task.Delay(20, token);
                result = result && await WriteAsync(inputFormatForAbsAngleBuffer, token);
                await Task.Delay(20, token);
                result = result && await WriteAsync(inputFormatForRelAngleBuffer, token);
                await Task.Delay(20, token);
                result = result && await WriteAsync(modeAndDataSetBuffer, token);
                await Task.Delay(20, token);
                result = result && await WriteAsync(unlockAndEnableBuffer, token);

                return result;
            }
            catch
            {
                return false;
            }
        }

        protected virtual async Task<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
        {
            try
            {
                baseAngle = Math.Max(-180, Math.Min(179, baseAngle));

                var resetToAngle = NormalizeAngle(GetAbsPosition(channel) - baseAngle);

                var result = true;

                result = result && await ResetAsync(channel, 0, token);
                result = result && await StopAsync(channel, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(50, token);
                result = result && await StopAsync(channel, token);
                result = result && await ResetAsync(channel, resetToAngle, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(500, token);
                result = result && await StopAsync(channel, token);

                var diff = Math.Abs(NormalizeAngle(GetAbsPosition(channel) - baseAngle));
                if (diff > 5)
                {
                    // Can't reset to base angle, rebase to current position not to stress the plastic
                    result = result && await ResetAsync(channel, 0, token);
                    result = result && await StopAsync(channel, token);
                    result = result && await TurnAsync(channel, 0, 40, token);
                    await Task.Delay(50, token);
                    result = result && await StopAsync(channel, token);
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(bool, float)> AutoCalibrateServoAsync(int channel, CancellationToken token)
        {
            try
            {
                var result = true;

                result = result && await ResetAsync(channel, 0, token);
                result = result && await StopAsync(channel, token);
                result = result && await TurnAsync(channel, 0, 50, token);
                await Task.Delay(600, token);
                result = result && await StopAsync(channel, token);
                await Task.Delay(500, token);
                var absPositionAt0 = GetAbsPosition(channel);
                result = result && await TurnAsync(channel, -160, 60, token);
                await Task.Delay(600, token);
                result = result && await StopAsync(channel, token);
                await Task.Delay(500, token);
                var absPositionAtMin160 = GetAbsPosition(channel);
                result = result && await TurnAsync(channel, 160, 60, token);
                await Task.Delay(600, token);
                result = result && await StopAsync(channel, token);
                await Task.Delay(500, token);
                var absPositionAt160 = GetAbsPosition(channel);

                var midPoint1 = NormalizeAngle((absPositionAtMin160 + absPositionAt160) / 2);
                var midPoint2 = NormalizeAngle(midPoint1 + 180);

                var baseAngle = (Math.Abs(NormalizeAngle(midPoint1 - absPositionAt0)) < Math.Abs(NormalizeAngle(midPoint2 - absPositionAt0))) ?
                    RoundAngleToNearest90(midPoint1) :
                    RoundAngleToNearest90(midPoint2);
                var resetToAngle = NormalizeAngle(GetAbsPosition(channel) - baseAngle);

                result = result && await ResetAsync(channel, 0, token);
                result = result && await StopAsync(channel, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(50, token);
                result = result && await StopAsync(channel, token);
                result = result && await ResetAsync(channel, resetToAngle, token);
                result = result && await TurnAsync(channel, 0, 40, token);
                await Task.Delay(600, token);
                result = result && await StopAsync(channel, token);

                return (result, baseAngle / 180F);
            }
            catch
            {
                return (false, 0F);
            }
        }

        private static int RoundAngleToNearest90(int angle)
        {
            angle = NormalizeAngle(angle);
            if (angle < -135) return -180;
            if (angle < -45) return -90;
            if (angle < 45) return 0;
            if (angle < 135) return 90;
            return -180;
        }

        private int CalculateServoSpeed(int channel, int targetAngle)
        {
            var position = ChannelRelativePositions.Exchange(channel, x => x.ConsumeUpdate());

            if (position.IsUpdated)
            {
                var diffAngle = Math.Abs(position.Current - targetAngle);
                return Math.Max(20, Math.Min(100, diffAngle));
            }

            if (position.UpdateTime == DateTime.MinValue ||
                POSITION_EXPIRATION < DateTime.Now - position.UpdateTime)
            {
                // Position update never happened or too old
                return 50;
            }

            return 0;
        }

        private Task<bool> StopAsync(int channel, CancellationToken token)
        {
            var portId = GetPortId(channel);
            return _bleDevice!.WriteAsync(Characteristic!, [0x08, 0x00, PORT_OUTPUT_COMMAND, portId, 0x11, PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, 0x00, 0x00], token);
        }

        private Task<bool> TurnAsync(int channel, int angle, int speed, CancellationToken token)
        {
            angle = NormalizeAngle(angle);
            var portId = GetPortId(channel);

            ToBytes(angle, out var a0, out var a1, out var a2, out var a3);
            return _bleDevice!.WriteAsync(Characteristic!, [0x0e, 0x00, PORT_OUTPUT_COMMAND, portId, 0x11, 0x0d, a0, a1, a2, a3, (byte)speed, 0x64, 0x7e, 0x00], token);
        }

        private Task<bool> ResetAsync(int channel, int angle, CancellationToken token)
        {
            angle = NormalizeAngle(angle);
            var portId = GetPortId(channel);

            ToBytes(angle, out var a0, out var a1, out var a2, out var a3);
            return _bleDevice!.WriteAsync(Characteristic!, [0x0b, 0x00, PORT_OUTPUT_COMMAND, portId, 0x11, PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, 0x02, a0, a1, a2, a3], token);
        }
    }
}
