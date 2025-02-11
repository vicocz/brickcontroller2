using BrickController2.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Dictionary of available gamepads having <see cref="IGameController.ControllerId"/>
    /// </summary>
    private readonly ObservableCollection<TGameController> _availableControllers = [];

    protected GameControllerServiceBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract bool IsControllerIdSupported { get; }

    public IReadOnlyCollection<IGameController> AvailableControllers => _availableControllers;

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

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _availableControllers.CollectionChanged += value;
        remove => _availableControllers.CollectionChanged -= value;
    }

    public void RaiseEvent(GameControllerEventArgs eventArgs)
    {
        GameControllerEventInternal?.Invoke(this, eventArgs);
    }

    public bool TryGetController(string id, [MaybeNullWhen(false)] out IGameController controller)
    {
        lock (_lockObject)
        {
            if (TryGetController(x => x.ControllerId == id, out var item))
            {
                controller = item;
                return true;
            }
            controller = default;
            return false;
        }
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
        foreach (var controller in _availableControllers)
        {
            controller.Stop();
        }
        _availableControllers.Clear();
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
            // handle possible situation with duplicated controller
            if (_availableControllers.Remove(x => x.ControllerId == controller.ControllerId, out var oldController))
            {
                _logger.LogDebug("Old duplicite gamepad was removed. ControllerId:{id}", oldController.ControllerId);
                oldController.Stop();
            }
            _availableControllers.Add(controller);
            controller.Start();
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
                return true;
            }
            return false;
        }
    }

    protected bool TryGetController(Predicate<TGameController> predicate, [MaybeNullWhen(false)] out TGameController controller)
    {
        lock (_lockObject)
        {
            // if there is no listener, block any access 
            if (GameControllerEventInternal != null)
            {
                controller = _availableControllers.FirstOrDefault(x => predicate(x));
                return controller is not null;
            }

            controller = default;
            return false;
        }
    }
}