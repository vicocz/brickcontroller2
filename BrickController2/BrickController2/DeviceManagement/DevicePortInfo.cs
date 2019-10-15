namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// Generic port info
    /// </summary>
    public class DevicePortInfo
    {
        public DevicePortInfo(string name, byte channel)
        {
            Name = name;
            Channel = channel;
        }

        public string Name { get; }
        public byte Channel { get; }

        public override string ToString()
        {
            return $"Port: {Name}, ID:{Channel}";
        }
    }
}