using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : ControlPlusDevice
    {
        public PoweredUpDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;
        public override int NumberOfChannels => 2;

        protected override void RegisterDefaultPorts()
        {
            RegisterPorts(new[]
            {
                new DevicePort(0, "A"),
                new DevicePort(1, "B"),
            });
        }

        protected override bool ProcessInternalSensorMessage(byte portNumber, byte[] message)
        {
            if (portNumber == 0x3c)
            {
                // Voltage
                var voltageRaw = message.ReadUInt16LE(4);
                var voltage = 9.620f * voltageRaw / 3893.0f;
                BatteryVoltage = voltage.ToString("F0");

                // DEBUG logging
                //System.Diagnostics.Debug.WriteLine($"[MessageType:Internal Sensor: {portNumber:X} Voltage: {Voltage}");

                return true;
            }

            return base.ProcessInternalSensorMessage(portNumber, message);
        }
    }
}
