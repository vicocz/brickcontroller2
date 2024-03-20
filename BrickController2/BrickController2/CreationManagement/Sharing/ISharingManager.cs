namespace BrickController2.CreationManagement.Sharing;

public interface ISharingManager<TModel> where TModel : class, IShareable
{
    /// <summary>
    /// Shares json model of <typeparamref name="TModel"/> to clipboard
    /// </summary>
    Task ShareToClipboardAsync(TModel model);

    /// <summary>
    /// Export the specified <paramref name="model"/> as JSON.
    /// </summary>
    Task<string> ShareAsync(TModel model);

    /// <summary>
    /// Imports the content of clipboard as json model of <typeparamref name="TModel"/>
    /// </summary>
    Task<TModel> ImportFromClipboardAsync();

    /// <summary>
    /// Imports the content of json model of <typeparamref name="TModel"/>
    /// </summary>
    TModel Import(string json);
}
