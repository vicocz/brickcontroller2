namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// Represents immutable description of a device port
    /// </summary>
    public class DevicePort
    {
        public DevicePort(byte channel, string name)
        {
            Channel = channel;
            Name = name;
        }

        public byte Channel { get; }

        public string Name { get; }

        public DevicePortType PortType { get; private set; }

        public bool IsConnected { get; private set; }

        public void SetDisconnected()
        {
            IsConnected = false;
            PortType = DevicePortType.Unknown;
        }

        public void SetConnected(DevicePortType portType = DevicePortType.Unknown)
        {
            IsConnected = true;
            PortType = portType;
        }

        public override string ToString()
        {
            return $"Port: {Name}, ID:{Channel}, IsConnected:{IsConnected}, PortType:{PortType}";
        }
    }
}
