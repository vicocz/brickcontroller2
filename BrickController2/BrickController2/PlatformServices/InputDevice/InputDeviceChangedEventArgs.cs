using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.PlatformServices.InputDevice;

public class InputDeviceChangedEventArgs : EventArgs
{
    public InputDeviceChangedEventArgs(NotifyInputDevicessChangedAction action, IEnumerable<IInputDevice> controllers)
    {
        Action = action;
        Items = controllers.ToArray();
    }

    public InputDeviceChangedEventArgs(NotifyInputDevicessChangedAction action, IInputDevice controller)
    {
        Action = action;
        Items = [controller];
    }

    public NotifyInputDevicessChangedAction Action { get; }
    public IReadOnlyCollection<IInputDevice> Items { get; }
}

public enum NotifyInputDevicessChangedAction
{
    Connected = 0,
    Disconnected = 1
}
