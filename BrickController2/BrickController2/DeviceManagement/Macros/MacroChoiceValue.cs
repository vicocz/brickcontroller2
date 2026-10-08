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

            case string text when typeof(T).IsEnum:
                if (Enum.TryParse(typeof(T), text, ignoreCase: true, out var parsed) && Enum.IsDefined(typeof(T), parsed))
                {
                    value = (T)parsed;
                    return true;
                }
                value = default!;
                return false;

            case var integral when typeof(T).IsEnum && IsIntegral(integral):
                try
                {
                    var enumValue = Enum.ToObject(typeof(T), integral!);
                    if (typeof(T).IsEnumDefined(enumValue))
                    {
                        value = (T)enumValue;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                }
                value = default!;
                return false;

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
    /// Compares a saved value against this (reference) value by converting the saved value
    /// to the reference type, tolerating type drift caused by (de)serialization
    /// (e.g. int -> long, float -> double). Strings are compared ordinally.
    /// Not symmetric: call it on the reference value (e.g. the descriptor's choice).
    /// </summary>
    public bool ValueEquals(MacroChoiceValue other) => (Value, other.Value) switch
    {
        (null, null) => true,
        (null, _) or (_, null) => false,
        (string a, _) => other.Value is string b && string.Equals(a, b, StringComparison.Ordinal),
        (bool a, _) => other.Value is bool b && a == b,
        (int a, _) => IsIntegral(other.Value) && other.TryGet<long>(out var b) && a == b,
        (long a, _) => IsIntegral(other.Value) && other.TryGet<long>(out var b) && a == b,
        (float a, _) => IsNumeric(other.Value) && other.TryGet<float>(out var b) && a.Equals(b),
        (double a, _) => IsNumeric(other.Value) && other.TryGet<double>(out var b) && a.Equals(b),
        (Enum a, _) => EnumEquals(a, other.Value),
        _ => Value.Equals(other.Value)
    };

    private static bool EnumEquals(Enum reference, object? saved) => saved switch
    {
        Enum e => reference.Equals(e),
        string s => Enum.TryParse(reference.GetType(), s, ignoreCase: true, out var parsed) && reference.Equals(parsed),
        _ when IsIntegral(saved) => new MacroChoiceValue(saved).TryGet<long>(out var b) && Convert.ToInt64(reference) == b,
        _ => false
    };

    private static bool IsIntegral(object? value) => value is sbyte or byte or short or ushort or int or uint or long or ulong;

    private static bool IsNumeric(object? value) => IsIntegral(value) || value is float or double or decimal;

    public override string? ToString() => Value?.ToString();
}