using System;
using System.Text.Json.Serialization;

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

    public override string? ToString() => Value?.ToString();
}
