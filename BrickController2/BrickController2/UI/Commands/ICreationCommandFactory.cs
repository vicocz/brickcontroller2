using BrickController2.CreationManagement;
using BrickController2.UI.ViewModels;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

public interface ICreationCommandFactory : ICommandFactory<Creation>
{
    ICommand PlayCommand(PageViewModelBase viewModel, Creation item, ControllerProfile? profile = default);
    ICommand PlayCommand(PageViewModelBase viewModel);
    ICommand PlayProfileCommand(PageViewModelBase viewModel);
}
