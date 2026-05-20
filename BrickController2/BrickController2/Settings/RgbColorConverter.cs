using System;
using System.ComponentModel;
using System.Globalization;

namespace BrickController2.Settings;

public class RgbColorConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType switch
    {
        Type t when t == typeof(int) || t == typeof(long) => true,
        _ => base.CanConvertFrom(context, sourceType),
    };

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => value switch
    {
        int intValue => new RgbColor(intValue),
        long longValue => new RgbColor((int)longValue),
        _ => base.ConvertFrom(context, culture, value)
    };
}
