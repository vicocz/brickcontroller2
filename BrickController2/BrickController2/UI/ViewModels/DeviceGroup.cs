using System.Collections.Generic;
using BrickController2.DeviceManagement;

namespace BrickController2.UI.ViewModels
{
    public class DeviceGroup : List<DeviceEntry>
    {
        public DeviceType DeviceType { get; }

        public string GroupName { get; }

        public DeviceGroup(DeviceType deviceType, string groupName, List<DeviceEntry> deviceEntries) : base(deviceEntries)
        {
            GroupName = groupName;
            DeviceType = deviceType;
        }
    }
}
