using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.ViewModels;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

public interface ICommandFactory<TModel> where TModel : IShareable
{
    ICommand ExportItemAsFileCommand(PageViewModelBase viewModel, TModel item);
    ICommand ImportItemFromFileCommand(PageViewModelBase viewModel);
    ICommand ImportItemFromJsonFileCommand(PageViewModelBase viewModel);
    ICommand PasteItemFromClipboardCommand(PageViewModelBase viewModel);
    ICommand ShareToClipboardCommand(PageViewModelBase viewModel, TModel model);
    ICommand ShareAsJsonFileCommand(PageViewModelBase viewModel, TModel model);
    ICommand ShareAsTextCommand(PageViewModelBase viewModel, TModel model);
}
