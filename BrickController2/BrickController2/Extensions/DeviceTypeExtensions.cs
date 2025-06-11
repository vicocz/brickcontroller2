using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Vendors;
using System;
using System.Reflection;

namespace BrickController2.Extensions;

internal static class DeviceTypeExtensions
{
    public static DeviceVendor GetVendor(this DeviceType type)
    {
        var member = typeof(DeviceType).GetMember(Enum.GetName(type))[0];
        var attr = member.GetCustomAttribute<DeviceVendorAttribute>();
        return attr?.Vendor ?? DeviceVendor.Unknown;
    }
}
