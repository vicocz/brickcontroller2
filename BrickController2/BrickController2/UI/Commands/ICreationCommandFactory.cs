using BrickController2.CreationManagement;
using BrickController2.UI.ViewModels;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

public interface ICreationCommandFactory : ICommandFactory<Creation>
{
    ICommand PlayCommand(PageViewModelBase viewModel, Creation creation, ControllerProfile? controllerProfile = default!);

    ICommand PlayCreationCommand(PageViewModelBase viewModel);

    ICommand PlayControllerProfileCommand(PageViewModelBase viewModel);

    ICommand RemapDeviceCommand(PageViewModelBase viewModel, Creation creation);
}
