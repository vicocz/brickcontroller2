using BrickController2.PlatformServices.GameController;
using System;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels;
public class GameControllerGroupViewModel : ObservableCollection<GameControllerEventViewModel>, IComparable<GameControllerGroupViewModel>
{
    public GameControllerGroupViewModel(IGameController controller) : this (controller.ControllerId, controller)
    {
    }

    public GameControllerGroupViewModel(string controllerId, IGameController? controller)
    {
        ControllerId = controllerId;
        ControllerNumber = controller?.ControllerNumber ?? default;
        ControllerName = controller?.Name ?? "";
    }

    public string ControllerId { get; }
    public int ControllerNumber { get; }
    public string ControllerName { get; }

    public int CompareTo(GameControllerGroupViewModel? other)
    {
        if (other == null) return 1;
        return ControllerNumber.CompareTo(other.ControllerNumber);
    }
}
