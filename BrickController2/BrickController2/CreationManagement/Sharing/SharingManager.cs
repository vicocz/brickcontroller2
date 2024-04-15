using Newtonsoft.Json;

namespace BrickController2.CreationManagement.Sharing;

public class SharingManager<TModel> : ISharingManager<TModel> where TModel : class, IShareable
{
    public SharingManager()
    {
        // default options for JSON
        JsonOptions = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        // compact options using auto gzip converter
        CompactJsonOptions = new(JsonOptions);
        CompactJsonOptions.Converters.Add(new ShareablePayloadConverter<TModel>(CompactJsonOptions));
    }

    internal JsonSerializerSettings JsonOptions { get; }
    internal JsonSerializerSettings CompactJsonOptions { get; }

    /// <inheritdoc/>
    public Task<string> ShareAsync(TModel model) => ShareAsync(model, CompactJsonOptions);

    /// <summary>
    /// Export the specified <paramref name="item"/> as serialized JSON model
    /// </summary>
    internal static Task<string> ShareAsync(TModel model, JsonSerializerSettings options)
    {
        var payload = new ShareablePayload<TModel>(model);
        var json = JsonConvert.SerializeObject(payload, options);
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

    internal static TModel Import(string json, JsonSerializerSettings options)
    {
        var model = JsonConvert.DeserializeObject<ShareablePayload<TModel>>(json, options);

        if (model?.Payload is null)
            throw new InvalidOperationException("Invalid json data.");

        if (model.PayloadType != TModel.Type)
            throw new InvalidOperationException($"Invalid json data, PayloadType {model.PayloadType} does not match the expected value.");

        return model.Payload;
    }
}
