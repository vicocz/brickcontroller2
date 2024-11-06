using Newtonsoft.Json;
using System;

namespace BrickController2.DeviceManagement;

public record DeviceSetting
{
    /// <summary>Unique setting name</summary>
    [JsonProperty]
    public string Name { get; init; } = default!;

    /// <summary>Type of setting value</summary>
    [JsonIgnore]
    public Type Type => Value?.GetType() ?? typeof(void);

    /// <summary>Current setting value</summary>
    [JsonProperty]
    public object Value { get; set; } = default!;
}
