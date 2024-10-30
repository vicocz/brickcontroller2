using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class TechnicMoveDevice : ControlPlusDevice
    {
        private const byte PORT_DRIVE_MOTOR_1 = 0x32;
        private const byte PORT_DRIVE_MOTOR_2 = 0x33;
        private const byte PORT_STEERING_MOTOR = 0x34;
        private const byte PORT_6LEDS = 0x35;

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

        protected override byte GetChannelValue(int value)
        {
            // TODO fix max 100%
            return base.GetChannelValue(value);
        }

        protected override Task<bool> SendOutputValueAsync(int channel, int value, CancellationToken token = default)
        {
            // 6LED
            if (channel > 2)
            {
                var rawValue = (byte)Math.Abs(value);
                var ledMask = 1 << (channel - 3);
                var cmd = new byte[] { 9, 0x00, 0x81, PORT_6LEDS, 0x11, 0x51, 0x00, (byte)ledMask, rawValue };

                return WriteNoResponseAsync(cmd, token);
            }
            
            return base.SendOutputValueAsync(channel, value, token);
        }
    }
}
