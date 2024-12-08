using BrickController2.CreationManagement.Sharing;
using System.Threading;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

public interface ICommandFactory<TModel> where TModel : IShareable
{
    ICommand CreateExportItemAsFileCommand(TModel item, CancellationToken token);
    ICommand CreateImportItemFromFileCommand(CancellationToken token);
    ICommand CreateImportItemFromJsonFileCommand(CancellationToken token);
    ICommand CreatePasteItemFromClipboardCommand(CancellationToken token);
    ICommand CreateShareToClipboardCommand(TModel model);
    ICommand CreateShareAsJsonFileCommand(TModel model);
    ICommand CreateShareAsTextCommand(TModel model);
}
