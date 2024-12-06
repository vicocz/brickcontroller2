using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using System.Threading;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

public interface ICommandFactory<TModel> where TModel : IShareable
{
    ICommand CreateExportItemAsFileCommand(TModel item, CancellationToken token);
    ICommand CreateNavigateToSharePageCommand(TModel creation);
    ICommand CreateShareToClipboardCommand(TModel model);
    ICommand CreateShareAsJsonFileCommand(TModel model);
    ICommand CreateShareAsTextCommand(TModel model);
}
