using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.PlatformServices.GameController;

public abstract class GameControllerServiceBase : IGameControllerService
{
    private readonly object _lockObject = new();

    private event EventHandler<GameControllerEventArgs>? GameControllerEventInternal;

    protected GameControllerServiceBase()
    {
    }

    public event EventHandler<GameControllerEventArgs> GameControllerEvent
    {
        add
        {
            lock (_lockObject)
            {
                if (GameControllerEventInternal == null)
                {
                    InitializeControllers();
                }

                GameControllerEventInternal += value;
            }
        }

        remove
        {
            lock (_lockObject)
            {
                GameControllerEventInternal -= value;

                if (GameControllerEventInternal == null)
                {
                    TerminateControllers();
                }
            }
        }
    }

    public abstract bool IsControllerIdSupported { get; }

    protected internal void RaiseEvent(IDictionary<(GameControllerEventType, string), float> events, string controllerId)
    {
        if (!events.Any())
        {
            return;
        }

        GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(controllerId, events));
    }

    protected internal void RaiseEvent(string eventCode, GameControllerEventType eventType, float value, string controllerId)
    {
        GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(controllerId, eventType, eventCode, value));
    }

    /// <summary>
    /// Initialize collection of avilable controllers (including listening of connected/disconnected controller)
    /// </summary>
    protected abstract void InitializeControllers();

    protected abstract void TerminateControllers();
}