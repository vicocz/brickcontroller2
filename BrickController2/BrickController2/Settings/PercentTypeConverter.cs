using System;
using System.ComponentModel;
using System.Globalization;

namespace BrickController2.Settings;

/// <summary>
/// Enables generic conversion into/from <see cref="Percent"/> (e.g. from a stored/deserialized
/// <see cref="float"/> or <see cref="long"/>), so it participates in the same conversion pipeline
/// used by <see cref="NamedSettingExtensions.GetValue{TValue}(NamedSetting?, TValue)"/>.
/// </summary>
internal class PercentTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(double) || sourceType == typeof(float) || sourceType == typeof(int) ||
           sourceType == typeof(long) || sourceType == typeof(string) ||
           base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value switch
        {
            Percent percent => percent,
            // stored/serialized values are persisted as plain (culture-invariant) floats, see ConvertTo
            string stringValue => new Percent(float.Parse(stringValue.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture)),
            IConvertible convertible => new Percent(convertible.ToSingle(culture)),
            _ => base.ConvertFrom(context, culture, value)
        };

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(float) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        => value is Percent percent
            ? destinationType == typeof(float)
                ? percent.Value
                : destinationType == typeof(string)
                    // persist as a plain, culture-invariant float string, not Percent.ToString()'s "N%" display format
                    ? percent.Value.ToString(CultureInfo.InvariantCulture)
                    : base.ConvertTo(context, culture, value, destinationType)
            : base.ConvertTo(context, culture, value, destinationType);
}
