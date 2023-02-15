using System;

using global::System.ComponentModel;
//#if NETSTANDARD2_0 || NETSTANDARD2_1

//// ReSharper disable once CheckNamespace
//namespace System.Runtime.CompilerServices
//{
//    /// <summary>
//    /// Reserved to be used by the compiler for tracking metadata.
//    /// This class should not be used by developers in source code.
//    /// </summary>
//    [EditorBrowsable(EditorBrowsableState.Never)]
//    internal static class IsExternalInit
//    {
//    }
//}

//#endif

namespace BrickController2.DeviceManagement
{


    public record DeviceSetting
    {
        /// <summary>Unique setting name</summary>
        public string SettingName { get; set; }
        
        /// <summary>Type of setting value</summary>
        public Type Type { get; set; }

        /// <summary>Default value</summary>
        public object DefaultValue { get; set; }
    }
}
