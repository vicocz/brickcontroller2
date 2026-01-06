using System;
using System.ComponentModel;

namespace BrickController2.Settings;

public static class NamedSettingExtensions
{
    public static TValue GetValue<TValue>(this NamedSetting? setting, TValue defaultValue)
    {
        if (setting == null || setting.Value is null)
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

        if (setting.Value is TValue typedValue)
            return typedValue;

        var converter = TypeDescriptor.GetConverter(typeof(TValue));
        if (converter.CanConvertFrom(setting.Value.GetType()) &&
            converter.ConvertFrom(setting.Value) is TValue convertedValue)
        {
            return convertedValue;
        }

        return (TValue)Convert.ChangeType(setting.Value, typeof(TValue));
    }
}
