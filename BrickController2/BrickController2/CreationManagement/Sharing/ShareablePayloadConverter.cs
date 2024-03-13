using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BrickController2.CreationManagement.Sharing;

internal class ShareablePayloadConverter<TModel> : JsonConverter<ShareablePayload<TModel>>
    where TModel : class, IShareable
{
    // reasonable value to optimize both json text readibility and pixel size of rendered QR
    private const int MaxSize = 1600;

    public override ShareablePayload<TModel> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        TModel payload = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new(payload);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var propertyName = reader.GetString();
            reader.Read();
            switch (propertyName)
            {
                case ShareablePayload<TModel>.ContentTypeProperty:
                    // validate expected content type
                    var contentType = reader.GetString();
                    if (contentType != TModel.Type)
                        throw new JsonException($"Unsuppported conent type: {contentType}.");
                    break;
                case ShareablePayload<TModel>.PayloadProperty:
                    // load payload
                    payload = DeserializePayload(ref reader, options);
                    break;

                default:
                    // unknown property
                    throw new JsonException($"Unexpected property: {propertyName}.");
            }
        }

        throw new JsonException();
    }

    private static TModel DeserializePayload(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                // directly deserialize payload
                return JsonSerializer.Deserialize<TModel>(ref reader, options);

            case JsonTokenType.String:
                // unzip Base64 payload string
                {
                    using var output = new MemoryStream();
                    using var input = new MemoryStream(reader.GetBytesFromBase64());
                    using var unzip = new GZipStream(input, CompressionMode.Decompress);
                    unzip.CopyTo(output);
                    output.Position = 0;

                    return JsonSerializer.Deserialize<TModel>(output, options);
                }

            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, ShareablePayload<TModel> model, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        // write content type of the model
        writer.WriteString(ShareablePayload<TModel>.ContentTypeProperty, TModel.Type);
        // write payload based on size autodetection
        var payload = JsonSerializer.Serialize(model.Payload, options);

        writer.WritePropertyName(ShareablePayload<TModel>.PayloadProperty);

        // autodetect final format based on source payload size
        if (payload.Length < MaxSize)
        {
            writer.WriteRawValue(payload);
        }
        else
        {
            using var output = new MemoryStream();
            using var zip = new GZipStream(output, CompressionMode.Compress);
            zip.Write(Encoding.UTF8.GetBytes(payload));
            zip.Flush();

            output.TryGetBuffer(out var buffer);
            writer.WriteBase64StringValue(buffer.AsSpan());
        }

        writer.WriteEndObject();
    }
}
