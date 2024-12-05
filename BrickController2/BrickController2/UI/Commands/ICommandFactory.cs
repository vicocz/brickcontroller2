using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

public interface ICommandFactory<TModel> where TModel : IShareable
{
    ICommand CreateShareToClipboardCommand(TModel model);
    ICommand CreateShareAsJsonFileCommand(TModel model);
    ICommand CreateShareAsTextCommand(TModel model);
    ICommand CreateNavigateToSharePageCommand(Creation creation);
}
