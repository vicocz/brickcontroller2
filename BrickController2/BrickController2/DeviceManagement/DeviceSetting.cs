using System;

namespace BrickController2.DeviceManagement
{
    public struct DeviceSetting
    {
        /// <summary>Unique setting name</summary>
        public string SettingName { get; set; }
        
        /// <summary>Type of setting value</summary>
        public Type Type { get; set; }

        /// <summary>Default value</summary>
        public object DefaultValue { get; set; }
    }
}
