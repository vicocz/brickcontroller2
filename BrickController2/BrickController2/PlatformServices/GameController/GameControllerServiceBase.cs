using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BrickController2.PlatformServices.GameController;

public abstract class GameControllerServiceBase<TKey, TGamepad, TGameController> : IGameControllerService
    where TKey : notnull
    where TGamepad : class
    where TGameController : GamepadControllerBase<TGamepad>
{
    private readonly object _lockObject = new();
    private readonly Dictionary<TKey, TGameController> _availableControllers = [];

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

    public abstract bool IsControllerIdSupported { get; }

    /// <summary>
    /// Get copy of all available keys of registered controllers
    /// </summary>
    protected IEnumerable<TKey> AllControllerKeys => [.. _availableControllers.Keys];

    /// <summary>
    /// returns the first unused index of device in controller management
    /// </summary>
    /// <returns>first unused index</returns>
    protected int GetFirstUnusedControllerIndex()
    {
        lock (_lockObject)
        {
            int unusedIndex = 0;
            while (_availableControllers.Values.Any(gamepadController => gamepadController.ControllerIndex == unusedIndex))
            {
                unusedIndex++;
            }
            return unusedIndex;
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
            _availableControllers[key] = controller;
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
            if (GameControllerEventInternal == null ||
                !_availableControllers.TryGetValue(key, out controller))
            {
                controller = default;
                return false;
            }

            return true;
        }
    }

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
    protected abstract void InitializeCurrentControllers();

    protected abstract void RemoveAllControllers();
}