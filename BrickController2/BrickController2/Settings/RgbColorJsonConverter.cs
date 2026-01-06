using Newtonsoft.Json;
using System;

namespace BrickController2.Settings;

public class RgbColorJsonConverter : JsonConverter<RgbColor>
{
    public override RgbColor ReadJson(JsonReader reader, Type objectType, RgbColor existingValue, bool hasExistingValue, JsonSerializer serializer)
        => throw new InvalidOperationException("Deserialization of RgbColor from JSON is intentionally not supported.");

    public override void WriteJson(JsonWriter writer, RgbColor value, JsonSerializer serializer)
        => writer.WriteValue(value.ToInt());
}
