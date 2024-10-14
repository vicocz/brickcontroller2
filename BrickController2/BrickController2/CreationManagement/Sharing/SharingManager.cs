using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement.Sharing;

public class SharingManager<TModel> : ISharingManager<TModel> where TModel : class, IShareable
{
    public SharingManager()
    {
        // default options for JSON
        JsonOptions = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
    }

    internal JsonSerializerSettings JsonOptions { get; }

    /// <summary>
    /// Export the specified <paramref name="item"/> as serialized JSON model
    /// </summary>
    internal Task<string> ShareAsync(TModel model)
    {
        var payload = new ShareablePayload<TModel>(model);
        var json = JsonConvert.SerializeObject(payload, JsonOptions);
        return Task.FromResult(json);
    }

    /// <inheritdoc/>
    public async Task ShareToClipboardAsync(TModel model)
    {
        var json = await ShareAsync(model);
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Clipboard.SetTextAsync(json);
        });
    }

    /// <inheritdoc/>
    public async Task<TModel> ImportFromClipboardAsync()
    {
        var json = await MainThread.InvokeOnMainThreadAsync(Clipboard.GetTextAsync);
        return Import(json);
    }

    internal TModel Import(string? json)
    {
        if (json is null)
            throw new InvalidOperationException("No json data.");

        var model = JsonConvert.DeserializeObject<ShareablePayload<TModel>>(json, JsonOptions);

        if (model?.Payload is null)
            throw new InvalidOperationException("Invalid json data.");

        if (model.PayloadType != TModel.Type)
            throw new InvalidOperationException($"Invalid json data, PayloadType {model.PayloadType} does not match the expected value.");

        return model.Payload;
    }
}
