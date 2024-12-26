using Newtonsoft.Json;
using System;

namespace BrickController2.DeviceManagement;

public record DeviceSetting
{
    /// <summary>Unique setting name</summary>
    [JsonProperty]
    public string Name { get; init; } = default!;

    /// <summary>Current setting value</summary>
    [JsonProperty]
    public object Value { get; set; } = default!;

    /// <summary>Type of setting value</summary>
    [JsonIgnore]
    public Type Type => Value?.GetType() ?? typeof(void);

    [JsonIgnore]
    public bool IsBoolType => Type == typeof(bool);

    [JsonIgnore]
    public bool IsEnumType => Type.IsEnum;
}
