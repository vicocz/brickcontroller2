namespace BrickController2.CreationManagement.Sharing;

public interface ISharingManager<TModel> where TModel : class, IShareable
{
    /// <summary>
    /// Export the specified <paramref name="model"/> as JSON.
    /// </summary>
    /// <typeparam name="TItem">Type of sharable item</typeparam>
    /// <returns>JSON payload that represents the specified model</returns>
    Task<string> ShareAsync(TModel model);

    Task ShareToClipboardAsync(TModel model);
 
    TModel Import(string json);
    Task<TModel> ImportFromClipboardAsync();
    Task<TModel> ImportFromFileAsync(string filePath);
}
