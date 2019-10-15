namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// Represents port of bluetooth device
    /// </summary>
    public class DevicePort : DevicePortInfo
    {
        public DevicePort(DevicePortInfo portTemplate) : base(portTemplate.Name, portTemplate.Channel)
        {
        }

        public DevicePort(string name, byte channel) : base(name, channel)
        {
        }

        public DevicePort(byte channel, string name) : base(name, channel)
        {
        }

        public DevicePortType PortType { get; private set; }

        public bool IsConnected { get; private set; }


        //TODO
        public bool busy;

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