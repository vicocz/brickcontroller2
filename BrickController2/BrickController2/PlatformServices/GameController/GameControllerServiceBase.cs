using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BrickController2.PlatformServices.GameController;

/// <summary>
/// Base class for implementation of <see cref="IGameControllerService"/>
/// </summary>
public abstract class GameControllerServiceBase : IGameControllerService
{
    protected readonly object _lockObject = new();
    protected readonly ILogger _logger;

    private event EventHandler<GameControllerEventArgs>? GameControllerEventInternal;

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

    internal void RaiseEvent(GameControllerEventArgs eventArgs)
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
    protected abstract void RemoveAllControllers();
}

public abstract class GameControllerServiceBase<TKey, TGamepad, TGameController> : GameControllerServiceBase
    where TKey : notnull
    where TGamepad : class
    where TGameController : GamepadControllerBase<TGamepad>
{
    private readonly Dictionary<TKey, TGameController> _availableControllers = [];

    protected GameControllerServiceBase(ILogger logger) : base (logger)
    {
    }

    /// <summary>
    /// Get copy of all available keys of registered controllers
    /// </summary>
    protected IEnumerable<TKey> AllControllerKeys => [.. _availableControllers.Keys];

    /// <summary>
    /// returns the first unused number of device in controller management
    /// </summary>
    /// <returns>first unused index</returns>
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


    /// <summary>
    /// Find key for registered native gamepad-instance.
    /// </summary>
    /// <param name="gamepad">gamepad-instance to find</param>
    /// <returns>key or null</returns>
    protected TKey GetKey(TGamepad gamepad)
    {
        lock (_lockObject)
        {
            return _availableControllers.FirstOrDefault(entry => entry.Value.Gamepad == gamepad).Key;
        }
    }

    protected void AddController(TKey key, TGameController controller)
    {
        lock (_lockObject)
        {
            // handle possible situation with duplicated controller
            if (_availableControllers.Remove(key, out var oldController))
            {
                oldController.Stop();
            }
            _availableControllers[key] = controller;
            controller.Start();
        }
    }

    protected void RemoveController(TKey key)
    {
        lock (_lockObject)
        {
            // remove and stop the controller
            if (_availableControllers.Remove(key, out var controller))
            {
                controller.Stop();
            }
        }
    }

    protected bool TryGetActiveController(TKey key, [MaybeNullWhen(false)] out TGameController controller)
    {
        lock (_lockObject)
        {
            if (//TODO GameControllerEventInternal == null ||
                !_availableControllers.TryGetValue(key, out controller))
            {
                controller = default;
                return false;
            }

            return true;
        }
    }

}