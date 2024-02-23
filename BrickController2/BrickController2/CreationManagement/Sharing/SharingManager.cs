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

    public JsonSerializerOptions JsonOptions { get; }
    public JsonSerializerOptions CompactJsonOptions { get; }

    /// <summary>
    /// Export the specified <paramref name="item"/> as compact JSON.
    /// </summary>
    /// <returns>JSON payload that represents the specified model</returns>
    public Task<string> ShareAsync(TModel model) => ShareAsync(model, CompactJsonOptions);

    internal static Task<string> ShareAsync(TModel model, JsonSerializerOptions options)
    {
        var payload = new ShareablePayload<TModel>(model);
        var json = JsonSerializer.Serialize(payload, options);
        return Task.FromResult(json);
    }

    public async Task ShareToClipboardAsync(TModel model)
    {
        var json = await ShareAsync(model, JsonOptions);
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Clipboard.Default.SetTextAsync(json);
        });
    }

    public async Task<TModel> ImportFromClipboardAsync()
    {
        var json = await MainThread.InvokeOnMainThreadAsync(Clipboard.Default.GetTextAsync);
        return Import(json, JsonOptions);
    }

    public async Task<TModel> ImportFromFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<TModel>(json, JsonOptions);
    }

    public TModel Import(string json) => Import(json, CompactJsonOptions);

    internal static TModel Import(string json, JsonSerializerOptions options)
    {
        var model = JsonSerializer.Deserialize<ShareablePayload<TModel>>(json, options);

        if (model?.Payload is null || model.PayloadType != TModel.Type)
            throw new InvalidOperationException();

        return model.Payload;
    }
}
