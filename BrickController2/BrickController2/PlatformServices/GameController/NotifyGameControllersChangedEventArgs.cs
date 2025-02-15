using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.PlatformServices.GameController;

public class NotifyGameControllersChangedEventArgs : EventArgs
{
    public NotifyGameControllersChangedEventArgs(NotifyGameControllersChangedAction action, IEnumerable<IGameController> controllers)
    {
        Action = action;
        Items = controllers.ToArray();
    }

    public NotifyGameControllersChangedEventArgs(NotifyGameControllersChangedAction action, IGameController controller)
    {
        Action = action;
        Items = [controller];
    }

    public NotifyGameControllersChangedAction Action { get; }
    public IReadOnlyCollection<IGameController> Items { get; }
}

public enum NotifyGameControllersChangedAction
{
    Connected = 0,
    Disconnected = 1
}
