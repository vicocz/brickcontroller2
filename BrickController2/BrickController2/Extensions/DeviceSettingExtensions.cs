using BrickController2.DeviceManagement;
using System;

namespace BrickController2.Extensions;

public static class DeviceSettingExtensions
{
    public static TValue GetValue<TValue>(this DeviceSetting? setting, TValue defaultValue)
    {
        if (setting == null)
            return defaultValue;

        // special handling of enums
        if (typeof(TValue).IsEnum)
        {
            var safeValue = Convert.ChangeType(setting.Value, Enum.GetUnderlyingType(typeof(TValue)));
            if (Enum.IsDefined(typeof(TValue), safeValue))
            {
                return (TValue)Enum.ToObject(typeof(TValue), safeValue);
            }
            return defaultValue;
        }

        return (TValue)setting.Value;
    }
}
