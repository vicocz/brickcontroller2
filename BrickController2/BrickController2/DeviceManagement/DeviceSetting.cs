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

    public TValue GetValue<TValue>(TValue defaultValue)
    {
        // special handling of enums
        if (typeof(TValue).IsEnum)
        {
            var safeValue = Convert.ChangeType(Value, Enum.GetUnderlyingType(typeof(TValue)));
            if (Enum.IsDefined(typeof(TValue), safeValue))
            {
                return (TValue)Enum.ToObject(typeof(TValue), safeValue);
            }
        }
        else
        {
            return (TValue)Value;
        }
        return defaultValue;
    }
}
