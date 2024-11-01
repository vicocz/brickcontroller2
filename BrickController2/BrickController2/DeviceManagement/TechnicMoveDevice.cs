using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Threading;
using System.Threading.Tasks;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement
{
    internal class TechnicMoveDevice : ControlPlusDevice
    {
        private const byte PORT_DRIVE_MOTOR_1 = 0x32;
        private const byte PORT_DRIVE_MOTOR_2 = 0x33;
        private const byte PORT_STEERING_MOTOR = 0x34;
        private const byte PORT_6LEDS = 0x35;
        private const byte PORT_PLAYVM = 0x36;

        private const byte PLAYVM_CALIBRATE_STEERING = 0x08;
        private const byte PLAYVM_COMMAND = 0x10;

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
            // special handling for PLAYVM
            PORT_PLAYVM => 2,
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
                return [9,
                    0x00, PORT_OUTPUT_COMMAND, PORT_6LEDS, FEEDBACK_ACTION_BOTH,
                    PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, PORT_MODE_0, ledMask, rawValue];
            }

            return base.GetOutputCommand(channel, value);
        }

        protected override byte[] GetServoCommand(int channel, int servoValue, int servoSpeed)
            => BuildPlayVmCmd(servoValue: servoValue);

        protected override async Task SetupServoAsync(int channel, int baseAngle, CancellationToken token = default)
        {
            // setup channel to report ABS position
            var portId = GetPortId(channel);
            var inputFormatForAbsAngle = BuildPortInputFormatSetup(portId, PORT_MODE_3);

            await WriteAsync(inputFormatForAbsAngle, token);
            await Task.Delay(100, token);

            // reset servo via PLAYVM
            // PLAYVM cmd supports only servo on C channel
            var servoCmd = BuildPlayVmCmd(servoValue: 0, vmCmd: PLAYVM_COMMAND);
            await WriteNoResponseAsync(servoCmd, token);
            await Task.Delay(100, token);

            // do calibration
            var calibrateCmd = BuildPlayVmCmd(servoValue: 0, vmCmd: PLAYVM_CALIBRATE_STEERING);
            await WriteNoResponseAsync(calibrateCmd, token);
            await Task.Delay(1500, token);
        }

        private static byte[] BuildPlayVmCmd(int speedValue = 0, int servoValue = 0, byte vmCmd = PLAYVM_COMMAND)
        {
            // PLAYVM cmd supports only servo on C channel
            var speedRaw = ToByte(speedValue);
            var steeringRaw = ToByte(servoValue);
            return [13,
                0x00, PORT_OUTPUT_COMMAND, PORT_PLAYVM, FEEDBACK_ACTION_BOTH,
                PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, PORT_MODE_0, 0x03, 0x00, speedRaw, steeringRaw, vmCmd, 0x00];
        }
    }
}
