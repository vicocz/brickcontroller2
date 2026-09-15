namespace BrickController2.DeviceManagement.Macros;

/// <summary>
/// Base type for a selectable macro choice. Create instances via <see cref="MacroChoice{T}"/>,
/// e.g. <c>new MacroChoice&lt;int&gt;(labelKey, 42)</c> or <c>new MacroChoice&lt;string&gt;(labelKey, "foo")</c>.
/// </summary>
public abstract record MacroChoice(string LabelKey)
{
    /// <summary>
    /// The choice value, wrapped as a <see cref="MacroChoiceValue"/>. Prefer the strongly-typed
    /// <see cref="MacroChoice{T}.TypedValue"/> when the concrete choice type is known at the call site.
    /// </summary>
    public abstract MacroChoiceValue Value { get; }

    /// <summary>
    /// Creates a <see cref="MacroChoice{T}"/> for a struct value, using the value's <see cref="object.ToString"/>
    /// as the label key.
    /// </summary>
    public static MacroChoice<T> Create<T>(T value) where T : struct
        => new(value.ToString() ?? string.Empty, value);
}

/// <summary>
/// A strongly-typed macro choice.
/// </summary>
public sealed record MacroChoice<T>(string LabelKey, T TypedValue) : MacroChoice(LabelKey)
{
    public override MacroChoiceValue Value => new(TypedValue);
}
