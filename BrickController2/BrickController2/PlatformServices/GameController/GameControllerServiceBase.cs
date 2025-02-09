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
    /// Dictionary of available gamepads having <see cref="IGameController.ControllerId"/>
    /// </summary>
    private readonly Dictionary<string, TGameController> _availableControllers = [];

    protected GameControllerServiceBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract bool IsControllerIdSupported { get; }

    public IReadOnlyCollection<IGameController> AvailableControllers => _availableControllers.Values;

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
        foreach (var controllerId in AllControllerIds)
        {
            RemoveController(controllerId);
        }
    }

    /// <summary>
    /// returns the first unused controller number in controller management
    /// </summary>
    protected int GetFirstUnusedControllerNumber()
    {
        lock (_lockObject)
        {
            int unusedNumber = 1;
            while (_availableControllers.Values.Any(gamepadController => gamepadController.ControllerNumber == unusedNumber))
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
            if (_availableControllers.Remove(controller.ControllerId, out var oldController))
            {
                //TODO _logger.LogDebug("Old duplicite gamepad was removed. UniqueId:{uniqueid}", oldController.UniquePersistantDeviceId);
                oldController.Stop();
            }
            _availableControllers[controller.ControllerId] = controller;
            controller.Start();
        }
    }

    protected void RemoveController(string controllerId)
    {
        lock (_lockObject)
        {
            // remove and stop the controller
            if (_availableControllers.Remove(controllerId, out var controller))
            {
                controller.Stop();
            }
        }
    }

    protected bool TryGetController(string controllerId, [MaybeNullWhen(false)] out TGameController controller)
    {
        lock (_lockObject)
        {
            // if there is no listener, block any access 
            if (GameControllerEventInternal == null ||
                !_availableControllers.TryGetValue(controllerId, out controller))
            {
                controller = default;
                return false;
            }

            return true;
        }
    }

    protected bool TryGetController(Func<TGameController, bool> predicate, [MaybeNullWhen(false)] out TGameController controller)
    {
        lock (_lockObject)
        {
            // if there is no listener, block any access 
            if (GameControllerEventInternal != null)
            {
                controller = _availableControllers.Values.FirstOrDefault(x => predicate(x));
                return controller is not null;
            }

            controller = default;
            return false;
        }
    }

    /// <summary>
    /// Get copy of all available keys of registered controllers
    /// </summary>
    protected IEnumerable<string> AllControllerIds => [.. _availableControllers.Keys];
}