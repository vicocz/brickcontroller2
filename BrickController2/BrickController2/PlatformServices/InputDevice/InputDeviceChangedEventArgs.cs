using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.PlatformServices.InputDevice;

public class InputDeviceChangedEventArgs : EventArgs
{
    public InputDeviceChangedEventArgs(NotifyInputDevicesChangedAction action, IEnumerable<IInputDevice> controllers)
    {
        Action = action;
        Items = controllers.ToArray();
    }

    public InputDeviceChangedEventArgs(NotifyInputDevicesChangedAction action, IInputDevice controller)
    {
        Action = action;
        Items = [controller];
    }

    public NotifyInputDevicesChangedAction Action { get; }
    public IReadOnlyCollection<IInputDevice> Items { get; }
}

public enum NotifyInputDevicesChangedAction
{
    Connected = 0,
    Disconnected = 1
}
