using BrickController2.PlatformServices.InputDevice;
using System;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels;

public class InputDeviceGroupViewModel : ObservableCollection<InputDeviceEventViewModel>, IComparable<InputDeviceGroupViewModel>
{
    public InputDeviceGroupViewModel(IInputDevice inputDevice) : this (inputDevice.InputDeviceId, inputDevice)
    {
    }

    public InputDeviceGroupViewModel(string inputDeviceId, IInputDevice? inputDevice)
    {
        InputDeviceId = inputDeviceId;
        InputDeviceNumber = inputDevice?.InputDeviceNumber ?? default;
        InputDeviceName = inputDevice?.Name ?? "";
    }

    public string InputDeviceId { get; }
    public int InputDeviceNumber { get; }
    public string InputDeviceName { get; }

    public int CompareTo(InputDeviceGroupViewModel? other)
    {
        if (other == null) return 1;
        return InputDeviceNumber.CompareTo(other.InputDeviceNumber);
    }
}
