using System;

namespace BrickController2.DeviceManagement;

public record DeviceSetting
{
    /// <summary>Unique setting name</summary>
    public string Name { get; init; } = default!;

    /// <summary>Type of setting value</summary>
    public Type Type => Value?.GetType() ?? typeof(void);

    /// <summary>Current setting value</summary>
    public object? Value { get; set; }
}
