using System;

namespace BrickController2.DeviceManagement.Vendors;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DeviceVendorAttribute(DeviceVendor vendor) : Attribute
{
    public DeviceVendor Vendor { get; } = vendor;
}
