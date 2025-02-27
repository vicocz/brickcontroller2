using System;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.DeviceManagement;

internal static class DeviceId
{
    public static string Get(DeviceType deviceType, string address) => $"{deviceType}#{address}";

    public static bool TryParse(string id, out DeviceType deviceType, [MaybeNullWhen(false)] out string address)
    {
        var deviceTypeAndAddress = id.Split('#');
        if (deviceTypeAndAddress.Length != 2 ||
            !Enum.TryParse<DeviceType>(deviceTypeAndAddress[0], out deviceType))
        {
            deviceType = DeviceType.Unknown;
            address =default;
            return false;
        }

        address = deviceTypeAndAddress[1];
        return true;
    }
}
