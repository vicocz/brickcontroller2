using BrickController2.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BrickController2.PlatformServices.GameController;

/// <summary>
/// Base class for implementation of <see cref="IGameControllerService"/>
/// </summary>
public abstract class GameControllerServiceBase<TGameController> : IGameControllerServiceInternal
    where TGameController : class, IGameController
{
    protected readonly object _lockObject = new();
    protected readonly ILogger _logger;

    private event EventHandler<GameControllerEventArgs>? GameControllerEventInternal;

    /// <summary>
    /// Collection of available gamepads having <see cref="IGameController.ControllerId"/>
    /// </summary>
    private readonly List<TGameController> _availableControllers = [];

    protected GameControllerServiceBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract bool IsControllerIdSupported { get; }

    public event EventHandler<GameControllerEventArgs> GameControllerEvent
    {
        add
        {
            lock (_lockObject)
            {
                if (GameControllerEventInternal == null)
                {
                    RemoveAllControllers();
                    InitializeCurrentControllers();
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
                    RemoveAllControllers();
                }
            }
        }
    }

    public event EventHandler<GameControllersChangedEventArgs>? GameControllersChangedEvent;

    protected bool CanProcessEvents
    {
        get
        {
            lock (_lockObject)
            {
                return GameControllerEventInternal != null;
            }
        }
    }

    public void RaiseEvent(GameControllerEventArgs eventArgs)
    {
        GameControllerEventInternal?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Initialize collection of avilable controllers (including listening of connected/disconnected controller)
    /// </summary>
    protected abstract void InitializeCurrentControllers();

    /// <summary>
    /// Remove and stop all available controllers
    /// </summary>
    /// <remarks>this is expected to be called under the lock</remarks>
    protected virtual void RemoveAllControllers()
    {
        if (_availableControllers.Count == 0)
        {
            return;
        }
        var controllers = _availableControllers.ToArray();
        foreach (var controller in _availableControllers)
        {
            controller.Stop();
        }
        _availableControllers.Clear();
        // notify removal
        OnGameControllersChangedEvent(NotifyGameControllersChangedAction.Disconnected, controllers);
    }

    /// <summary>
    /// returns the first unused controller number in controller management
    /// </summary>
    protected int GetFirstUnusedControllerNumber()
    {
        lock (_lockObject)
        {
            int unusedNumber = 1;
            while (_availableControllers.Any(gamepadController => gamepadController.ControllerNumber == unusedNumber))
            {
                unusedNumber++;
            }
            return unusedNumber;
        }
    }

    protected void AddController(TGameController controller)
    {
        lock (_lockObject)
        {
            _availableControllers.Add(controller);
            controller.Start();
            // notify adding
            OnGameControllersChangedEvent(NotifyGameControllersChangedAction.Connected, controller);
        }
    }

    protected bool TryRemove(Predicate<TGameController> predicate, [MaybeNullWhen(false)] out TGameController controller)
    {
        lock (_lockObject)
        {
            // remove and stop the controller
            if (_availableControllers.Remove(predicate, out controller))
            {
                controller.Stop();
                // notify removal
                OnGameControllersChangedEvent(NotifyGameControllersChangedAction.Disconnected, controller);
                return true;
            }
            return false;
        }
    }

    protected bool TryGetController(Predicate<TGameController> predicate, [MaybeNullWhen(false)] out TGameController controller)
    {
        lock (_lockObject)
        {
            controller = _availableControllers.FirstOrDefault(x => predicate(x));
            return controller is not null;
        }
    }

    private void OnGameControllersChangedEvent(NotifyGameControllersChangedAction action, params IGameController[] controllers)
    {
        GameControllersChangedEvent?.Invoke(this, new(action, controllers));
    }
}