using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
        public BoostDevice(
            string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;
        public override int NumberOfChannels => 4;

        protected override void RegisterDefaultPorts()
        {
            RegisterPorts(new[]
            {
                new DevicePort(0, "A"),
                new DevicePort(1, "B"),
                new DevicePort(2, "C"),
                new DevicePort(3, "D"),
            });
        }

        protected override bool ProcessInternalSensorMessage(byte portNumber, byte[] message)
        {
            if (portNumber == 58)
            {
                // InternalTilt
                //var tiltX = message[4] > 160 ? message[4] - 255 : message[4];
                //var tiltY = message[5] > 160 ? 255 - message[5] : message[5] - (message[5] * 2);

                var tiltX = (sbyte) message[4];
                var tiltY = (sbyte)message[5];

                var z = (sbyte)message[6];

                // DEBUG logging
                //System.Diagnostics.Debug.WriteLine($"[MessageType:Internal Sensor: {portNumber:X} TiltX: {tiltX}, TiltY: {tiltY} Z: {z}");

                return true;
            }

            return base.ProcessInternalSensorMessage(portNumber, message);
        }
    }
}
