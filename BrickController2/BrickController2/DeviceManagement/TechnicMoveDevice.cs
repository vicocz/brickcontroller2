using BrickController2.PlatformServices.BluetoothLE;
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
        private const int CHANNEL_VM = 12; // artificial channel to mimic combined AB ports in PLAYVM
        private const int CHANNEL_C = 2;

        private bool _applyPlayVmMode;
        private byte _virtualMotorValue;

        public TechnicMoveDevice(string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicMove;
        public override int NumberOfChannels => 9;

        // This is now mandatory as the hub does not support generic servo / stepper commands (yet)
        public bool EnablePlayVmMode => true;

        public override bool CanAutoCalibrateOutput(int channel) => channel == CHANNEL_C;
        public override bool CanResetOutput(int channel) => channel == CHANNEL_C;
        public override bool CanChangeOutputType(int channel) => channel == CHANNEL_C;

        public override Task<DeviceConnectionResult> ConnectAsync(bool reconnect, Action<Device> onDeviceDisconnected, IEnumerable<ChannelConfiguration> channelConfigurations, bool startOutputProcessing, bool requestDeviceInformation, CancellationToken token)
        {
            // autodetect PLAYVM mode for A / B channels (as testing page should not be affected)
            _applyPlayVmMode = startOutputProcessing &&
                channelConfigurations.Any(c => c.Channel == CHANNEL_VM);

            // filter out non standard channels
            var filteredConfigurtions = channelConfigurations
                .Where(c => c.Channel != CHANNEL_VM);

            return base.ConnectAsync(reconnect, onDeviceDisconnected, filteredConfigurtions, startOutputProcessing, requestDeviceInformation, token);
        }

        public override void SetOutput(int channel, float value)
        {
            if (channel == CHANNEL_VM)
            {
                // reset servo writes
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

        protected override int GetChannelIndex(byte portId) => portId switch
        {
            PORT_DRIVE_MOTOR_1 => 0,
            PORT_DRIVE_MOTOR_2 => 1,
            PORT_STEERING_MOTOR => 2,
            // PORT_6LEDS is not supported
            _ => throw new ArgumentException($"Value of port ID '{portId}' is out of supported ranges.", nameof(portId))
        };

        protected override byte GetChannelValue(int value) => ToByte(value);

        protected override void InitializeChannelInfo(int channel, int lastOutputValue = 1, int sendAttempsLeft = 10)
        {
            // if PLAYVM enabled, reset A / B channels diffrently in order to avoid output writes
            if (_applyPlayVmMode && channel < CHANNEL_C)
            {
                lastOutputValue = 0;
                sendAttempsLeft = 0;
            }
            base.InitializeChannelInfo(channel, lastOutputValue, sendAttempsLeft);
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
            return base.GetServoCommand(channel, servoValue, servoSpeed);
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                await WaitForPortSetupCompletedAsync(token);

                // switch lights off
                var lightsOffCmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, 0xff, 0x00);
                await WriteNoResponseAsync(lightsOffCmd, token);
                await SendDelayAsync(token);

                if (requestDeviceInformation)
                {
                    await RequestHubPropertiesAsync(token);
                }

                if (_applyPlayVmMode)
                {
                    // setup channel to report ABS position
                    var inputFormatForAbsAngle = BuildPortInputFormatSetup(PORT_STEERING_MOTOR, PORT_MODE_3);
                    await WriteAsync(inputFormatForAbsAngle, token);
                    await SendDelayAsync(token);

                    // reset servo via PLAYVM
                    // PLAYVM cmd supports only servo on C channel
                    var servoCmd = BuildPortOutput_PlayVm(servoValue: 0, vmCmd: PLAYVM_COMMAND);
                    await WriteNoResponseAsync(servoCmd, token);
                    await SendDelayAsync(token);

                    // do calibration
                    var calibrateCmd = BuildPortOutput_PlayVm(vmCmd: PLAYVM_CALIBRATE_STEERING);
                    await WriteNoResponseAsync(calibrateCmd, token);
                    await Task.Delay(1200, token);
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
