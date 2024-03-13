using System.Text.Json.Serialization;

namespace BrickController2.CreationManagement.Sharing;

/// <summary>
/// Wrapper for serialization of <typeparam name="TModel"></typeparam>
internal sealed class ShareablePayload<TModel> where TModel : class, IShareable
{
    public const string ContentTypeProperty = "ct";
    public const string PayloadProperty = "p";

    [JsonConstructor]
    private ShareablePayload()
    { 
    }

    internal ShareablePayload(TModel payload)
    {
        PayloadType = TModel.Type;
        Payload = payload;
    }

    [JsonPropertyName(ContentTypeProperty)]
    [JsonInclude]
    public string PayloadType { get; private set; }

    [JsonPropertyName(PayloadProperty)]
    [JsonInclude]
    public TModel Payload { get; private set; }
}
