using BrickController2.PlatformServices.InputDevice;
using System;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels;
public class GameControllerGroupViewModel : ObservableCollection<GameControllerEventViewModel>, IComparable<GameControllerGroupViewModel>
{
    public GameControllerGroupViewModel(IInputDevice controller) : this (controller.InputDeviceId, controller)
    {
    }

    public GameControllerGroupViewModel(string controllerId, IInputDevice? controller)
    {
        ControllerId = controllerId;
        ControllerNumber = controller?.InputDeviceNumber ?? default;
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
