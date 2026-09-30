using System;
using Newtonsoft.Json;

namespace BrickController2.DeviceManagement.Macros;

/// <summary>
/// Wraps the boxed value used for a macro choice/parameter (e.g. a sound file name, a volume level, ...).
/// Implicit conversions let callers assign/read plain values (<see cref="string"/>, <see cref="int"/>,
/// <see cref="float"/>, <see cref="bool"/>) directly, without manually boxing into <see cref="object"/>
/// or constructing this struct explicitly.
/// </summary>
public readonly record struct MacroChoiceValue(object? Value)
{
    [JsonIgnore]
    public bool HasValue => Value is not null;

    public static implicit operator MacroChoiceValue(int value) => new(value);
    public static implicit operator MacroChoiceValue(float value) => new(value);
    public static implicit operator MacroChoiceValue(bool value) => new(value);
    public static implicit operator MacroChoiceValue(string? value) => new(value);

    public bool TryGet<T>(out T value)
    {
        switch (Value)
        {
            case T typed:
                value = typed;
                return true;

            case IConvertible convertible when typeof(T).IsValueType:
                try
                {
                    value = (T)Convert.ChangeType(convertible, typeof(T));
                    return true;
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
                {
                    value = default!;
                    return false;
                }

            default:
                value = default!;
                return false;
        }
    }

    public T? As<T>() => TryGet<T>(out var value) ? value : default;

    /// <summary>
    /// Compares values tolerant to numeric type changes caused by (de)serialization
    /// (e.g. int/long/float/double), strings are compared ordinally.
    /// </summary>
    public bool ValueEquals(MacroChoiceValue other)
    {
        return (Value, other.Value) switch
        {
            (null, null) => true,
            (null, _) or (_, null) => false,
            (string a, string b) => string.Equals(a, b, StringComparison.Ordinal),
            (bool a, bool b) => a == b,
            ({ } a, { } b) when IsNumeric(a) && IsNumeric(b) => NumericEquals(a, b),
            _ => false
        };

        static bool IsNumeric(object value) => value is sbyte or byte or short or ushort or int or uint
            or long or ulong or float or double or decimal;

        static bool IsFloatingPoint(object value) => value is float or double;

        static bool NumericEquals(object a, object b)
        {
            if (!IsFloatingPoint(a) && !IsFloatingPoint(b))
            {
                // integral/decimal values are compared exactly
                return Convert.ToDecimal(a) == Convert.ToDecimal(b);
            }

            if (a is float || b is float)
            {
                // float vs double: serialization may change the type, compare with float precision
                // float vs integral: compare exactly as double to avoid collapsing large integers
                if (IsFloatingPoint(a) && IsFloatingPoint(b))
                {
                    return FloatEquals(Convert.ToSingle(a), Convert.ToSingle(b));
                }
            }

            var x = ToDouble(a);
            var y = ToDouble(b);
            return double.IsNaN(x) && double.IsNaN(y) || x == y;
        }

        static bool FloatEquals(float x, float y) => float.IsNaN(x) && float.IsNaN(y) || x == y;

        static double ToDouble(object value) => value is decimal d ? (double)d : Convert.ToDouble(value);
    }

    public override string? ToString() => Value?.ToString();
}