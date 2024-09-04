using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class TechnicMoveDevice : ControlPlusDevice
    {
        public TechnicMoveDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicMove;
        public override int NumberOfChannels => 3;

        protected override byte GetPortId(int channelIndex) => channelIndex switch
        {
            0 => 0x32,
            1 => 0x33,
            2 => 0x34,
            _ => throw new ArgumentException($"Value of channel '{channelIndex}' is out of supported range.", nameof(channelIndex))
        };

        protected override int GetChannelIndex(byte portId) => portId switch
        {
            0x32 => 0,
            0x33 => 1,
            0x34 => 2,
            _ => throw new ArgumentException($"Value of port ID '{portId}' is out of supported ranges.", nameof(portId))
        };
    }
}
