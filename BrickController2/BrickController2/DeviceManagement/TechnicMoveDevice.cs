using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Threading;
using System.Threading.Tasks;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement
{
    internal class TechnicMoveDevice : ControlPlusDevice
    {
        public TechnicMoveDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicMove;
        public override int NumberOfChannels => 9;

        public override bool CanAutoCalibrateOutput(int channel) => channel == 2;
        public override bool CanResetOutput(int channel) => channel == 2;
        public override bool CanChangeOutputType(int channel) => channel == 2;

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
            => BuildPortOutput_PlayVm(servoValue: servoValue);

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                // Wait until ports finish communicating with the hub
                await Task.Delay(1000, token);

                // switch lights off
                var lightsOffCmd = BuildPortOutput_LedMask(PORT_6LEDS, PORT_MODE_0, 0xff, 0x00);
                await WriteNoResponseAsync(lightsOffCmd, token);
                await SendDelayAsync(token);

                if (requestDeviceInformation)
                {
                    await RequestHubPropertiesAsync(token);
                }

                // TODO conditionally apply PLAYVM

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
                await Task.Delay(1500, token);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
