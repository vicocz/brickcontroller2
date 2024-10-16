using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BrickController2.CreationManagement.Sharing;

internal class ShareablePayloadConverter<TModel> : JsonConverter<ShareablePayload<TModel>>
    where TModel : class, IShareable
{
    // reasonable value to optimize both json text readibility and pixel size of rendered QR
    private const int MaxSize = 1600;
    private readonly JsonSerializerSettings _settings;

    public ShareablePayloadConverter(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public override ShareablePayload<TModel> ReadJson(JsonReader reader, Type objectType, ShareablePayload<TModel> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonException($"Incorrect payload format. TokenType:{reader.TokenType}");

        TModel payload = default!;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
                return new(payload);

            if (reader.TokenType != JsonToken.PropertyName)
                throw new JsonException($"Incorrect payload format. TokenType:{reader.TokenType}");

            var propertyName = reader.Value.ToString();
            switch (propertyName)
            {
                case ShareablePayload<TModel>.ContentTypeProperty:
                    // validate expected content type
                    var contentType = reader.ReadAsString();
                    if (contentType != TModel.Type)
                        throw new JsonException($"Unsuppported content type: {contentType}.");
                    break;
                case ShareablePayload<TModel>.PayloadProperty:
                    // load payload
                    if (reader.Read())
                    {
                        payload = DeserializePayload(reader, serializer);
                    }
                    break;

                default:
                    // unknown property
                    throw new JsonException($"Unexpected property: {propertyName}.");
            }
        }

        throw new JsonException("Incorrect payload format.");
    }

    private static TModel DeserializePayload(JsonReader reader, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                // directly deserialize payload
                return serializer.Deserialize<TModel>(reader);

            case JsonToken.String:
                // unzip Base64 payload string
                {
                    using var input = new MemoryStream(Convert.FromBase64String((string)reader.Value));
                    using var unzip = new GZipStream(input, CompressionMode.Decompress);
                    using var json = new StreamReader(unzip);
                    return (TModel)serializer.Deserialize(json, typeof(TModel));
                }

            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}.");
        }
    }

    public override void WriteJson(JsonWriter writer, ShareablePayload<TModel> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        // write content type of the model
        writer.WritePropertyName(ShareablePayload<TModel>.ContentTypeProperty);
        writer.WriteValue(TModel.Type);
        // write payload based on size autodetection
        var payload = JsonConvert.SerializeObject(value.Payload, _settings);

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

            // zipped byte[] is writen as base64
            writer.WriteValue(output.ToArray());
        }

        writer.WriteEndObject();
    }
}
