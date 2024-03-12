using System.Text.Json.Serialization;

namespace BrickController2.CreationManagement.Sharing;

/// <summary>
/// Wrapper for serialization of <typeparam name="TModel"></typeparam>
internal class ShareablePayload<TModel> where TModel : class, IShareable
{
    public const string ContentTypeProperty = "ct";
    public const string PayloadProperty = "p";

    private ShareablePayload()
    { 
    }

    public ShareablePayload(TModel payload)
    {
        PayloadType = TModel.Type;
        Payload = payload;
    }

    [JsonPropertyName(ContentTypeProperty)]
    public string PayloadType { get; private init; }

    [JsonPropertyName(PayloadProperty)]
    public TModel Payload { get; private init; }
}
