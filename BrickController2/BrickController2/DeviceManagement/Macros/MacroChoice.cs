namespace BrickController2.DeviceManagement.Macros;

/// <summary>
/// Base type for a selectable macro choice. Create instances via <see cref="MacroChoice{T}"/>,
/// e.g. <c>new MacroChoice&lt;int&gt;(labelKey, 42)</c> or <c>new MacroChoice&lt;string&gt;(labelKey, "foo")</c>.
/// </summary>
public abstract record MacroChoice(string LabelKey)
{
    /// <summary>
    /// The choice value, boxed. Prefer the strongly-typed <see cref="MacroChoice{T}.Value"/> when the
    /// concrete choice type is known at the call site.
    /// </summary>
    public abstract object BoxedValue { get; }
}

/// <summary>
/// A strongly-typed macro choice.
/// </summary>
public sealed record MacroChoice<T>(string LabelKey, T Value) : MacroChoice(LabelKey)
{
    public override object BoxedValue => Value!;
}
