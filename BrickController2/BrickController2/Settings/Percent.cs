using System.ComponentModel;

namespace BrickController2.Settings;

/// <summary>
/// A double value constrained to a percentage range (<see cref="MinValue"/>-<see cref="MaxValue"/>).
/// Used as a distinct <see cref="NamedSetting.Value"/> type so the settings UI can automatically
/// render it (e.g. as a slider) with the correct range, without requiring extra range metadata.
/// </summary>
[TypeConverter(typeof(PercentTypeConverter))]
public readonly record struct Percent
{
    public const float MinValue = 0f;
    public const float MaxValue = 100f;

    public Percent(float value) => Value = Clamp(value);

    public float Value { get; }

    public static implicit operator Percent(float value) => new(value);
    public static implicit operator float(Percent percent) => percent.Value;

    private static float Clamp(float value) => value < MinValue ? MinValue : value > MaxValue ? MaxValue : value;

    public override string ToString() => $"{Value:0}%";
}
