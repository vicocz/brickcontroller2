using System.Text.Json;
using System.Text.Json.Serialization;

namespace BrickController2.CreationManagement.Sharing;

public class SharingManager<TModel> : ISharingManager<TModel> where TModel : class, IShareable
{
    public SharingManager()
    {
        // default options for JSON
        JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };
        // compact options using auto gzip converter
        CompactJsonOptions = new JsonSerializerOptions(JsonOptions);
        CompactJsonOptions.Converters.Add(new ShareablePayloadConverter<TModel>());
    }

    internal JsonSerializerOptions JsonOptions { get; }
    internal JsonSerializerOptions CompactJsonOptions { get; }

    /// <inheritdoc/>
    public Task<string> ShareAsync(TModel model) => ShareAsync(model, CompactJsonOptions);

    /// <summary>
    /// Export the specified <paramref name="item"/> as serialized JSON model
    /// </summary>
    internal static Task<string> ShareAsync(TModel model, JsonSerializerOptions options)
    {
        var payload = new ShareablePayload<TModel>(model);
        var json = JsonSerializer.Serialize(payload, options);
        return Task.FromResult(json);
    }

    /// <inheritdoc/>
    public async Task ShareToClipboardAsync(TModel model)
    {
        var json = await ShareAsync(model, JsonOptions);
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Clipboard.SetTextAsync(json);
        });
    }

    /// <inheritdoc/>
    public async Task<TModel> ImportFromClipboardAsync()
    {
        var json = await MainThread.InvokeOnMainThreadAsync(Clipboard.GetTextAsync);
        return Import(json, JsonOptions);
    }

    /// <inheritdoc/>
    public TModel Import(string json) => Import(json, CompactJsonOptions);

    internal static TModel Import(string json, JsonSerializerOptions options)
    {
        var model = JsonSerializer.Deserialize<ShareablePayload<TModel>>(json, options);

        if (model?.Payload is null || model.PayloadType != TModel.Type)
            throw new InvalidOperationException("Invalid json data.");

        return model.Payload;
    }
}
