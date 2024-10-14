using System.Threading.Tasks;

namespace BrickController2.CreationManagement.Sharing;

public interface ISharingManager<TModel> where TModel : class, IShareable
{
    /// <summary>
    /// Shares json model of <typeparamref name="TModel"/> to clipboard
    /// </summary>
    Task ShareToClipboardAsync(TModel model);
 
    /// <summary>
    /// Imports the content of clipboard as json model of <typeparamref name="TModel"/>
    /// </summary>
    Task<TModel> ImportFromClipboardAsync();
}
