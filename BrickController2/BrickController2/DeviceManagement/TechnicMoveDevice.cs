using BrickController2.PlatformServices.BluetoothLE;
using System;

namespace BrickController2.DeviceManagement
{
    internal class TechnicMoveDevice : ControlPlusDevice
    {
        private const byte PORT_DRIVE_MOTOR_1 = 0x32;
        private const byte PORT_DRIVE_MOTOR_2 = 0x33;
        private const byte PORT_STEERING_MOTOR = 0x34;

        public TechnicMoveDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicMove;
        public override int NumberOfChannels => 3;

        public override bool CanAutoCalibrateOutput(int channel) => channel == 2;
        public override bool CanResetOutput(int channel) => channel == 2;

        protected override byte GetPortId(int channelIndex) => channelIndex switch
        {
            0 => PORT_DRIVE_MOTOR_1,
            1 => PORT_DRIVE_MOTOR_2,
            2 => PORT_STEERING_MOTOR,
            _ => throw new ArgumentException($"Value of channel '{channelIndex}' is out of supported range.", nameof(channelIndex))
        };

        protected override int GetChannelIndex(byte portId) => portId switch
        {
            PORT_DRIVE_MOTOR_1 => 0,
            PORT_DRIVE_MOTOR_2 => 1,
            PORT_STEERING_MOTOR => 2,
            _ => throw new ArgumentException($"Value of port ID '{portId}' is out of supported ranges.", nameof(portId))
        };
    }
}
