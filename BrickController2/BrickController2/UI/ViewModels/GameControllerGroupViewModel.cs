using BrickController2.PlatformServices.GameController;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels;
public class GameControllerGroupViewModel : ObservableCollection<GameControllerEventViewModel>
{
    public GameControllerGroupViewModel(string controllerId, IGameController? controller)
    {
        ControllerId = controllerId;
        ControllerNumber = controller?.ControllerNumber ?? default;
        ControllerName = controller?.Name ?? "";
    }

    public string ControllerId { get; }
    public int ControllerNumber { get; }
    public string ControllerName { get; }
}
