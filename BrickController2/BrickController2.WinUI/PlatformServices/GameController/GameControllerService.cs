using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Windows.Gaming.Input;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.MainThread;
using BrickController2.Windows.Extensions;

namespace BrickController2.Windows.PlatformServices.GameController;

public class GameControllerService : IGameControllerService
{
    private readonly Dictionary<string, GamepadController> _availableControllers = [];
    private readonly object _lockObject = new();
    private readonly IMainThreadService _mainThreadService;
    private readonly IDispatcherProvider _dispatcherProvider;

    private event EventHandler<GameControllerEventArgs>? GameControllerEventInternal;

    public GameControllerService(IMainThreadService mainThreadService, IDispatcherProvider dispatcherProvider)
    {
        _mainThreadService = mainThreadService;
        _dispatcherProvider = dispatcherProvider;
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

    public bool IsControllerIdSupported => true;

    internal void RaiseEvent(IDictionary<(GameControllerEventType, string), float> events, string controllerId)
    {
        if (!events.Any())
        {
            return;
        }

        GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(controllerId, events));
    }

    internal void RaiseEvent(string deviceId, string key, GameControllerEventType eventType, string controllerId, float value = 0.0f)
    {
        GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(controllerId, eventType, key, value));
    }

    private void InitializeControllers()
    {
        // get all available gamepads
        if (Gamepad.Gamepads.Any())
        {
            AddDevices(Gamepad.Gamepads);
        }

        Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded += Gamepad_GamepadAdded;
    }

    private void TerminateControllers()
    {
        Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded -= Gamepad_GamepadAdded;

        foreach (var controller in _availableControllers.Values)
        {
            controller.Stop();
        }
        _availableControllers.Clear();
    }

    private void Gamepad_GamepadRemoved(object? sender, Gamepad e)
    {
        lock (_lockObject)
        {
            // JK: UniquePersistentDeviceId is not available
            //var deviceId = e.GetUniquePersistentDeviceId();

            string deviceId = FindUniquePersistentDeviceId(e);

            if (deviceId is not null &&
                _availableControllers.TryGetValue(deviceId, out var controller))
            {
                _availableControllers.Remove(deviceId);

                // ensure stopped in UI thread
                _ = _mainThreadService.RunOnMainThread(() => controller.Stop());
            }
        }
    }

    private void Gamepad_GamepadAdded(object? sender, Gamepad e)
    {
        // ensure created in UI thread
        _ = _mainThreadService.RunOnMainThread(() => AddDevices([e]));
    }

    private void AddDevices(IEnumerable<Gamepad> gamepads)
    {
        lock (_lockObject)
        {
            var dispatcher = _dispatcherProvider.GetForCurrentThread();
            foreach (var gamepad in gamepads)
            {
                // deviceId looks like "{wgi/nrid/]Xd\\h-M1mO]-il0l-4L\\-Gebf:^3->kBRhM-d4}\0"
                string? uniquePersistentDeviceId = gamepad?.GetUniquePersistentDeviceId();

                if(string.IsNullOrEmpty(uniquePersistentDeviceId))
                {
                    continue;
                }

                int controllerIndex = GetFirstUnusedControllerIndex(); // get first unused index

                var newController = new GamepadController(this, gamepad!, controllerIndex, dispatcher!.CreateTimer());
                _availableControllers[uniquePersistentDeviceId] = newController;

                newController.Start();
            }
        }
    }

    /// <summary>
    /// returns the first unused index of device in controller management
    /// </summary>
    /// <returns>first unused index</returns>
    private int GetFirstUnusedControllerIndex()
    {
        lock (_lockObject)
        {
            int unusedIndex = 0;
            while(_availableControllers.Values.Any(gamepadController => gamepadController.ControllerIndex == unusedIndex))
            {
                unusedIndex++;
            }
            return unusedIndex;
        }
    }

    /// <summary>
    /// Find key for registered gamepad-instance.
    /// </summary>
    /// <param name="gamepad">gamepad-instance to find</param>
    /// <returns>key or null</returns>
    private string FindUniquePersistentDeviceId(Gamepad gamepad)
    {
        return _availableControllers.FirstOrDefault(entry => entry.Value.Gamepad == gamepad).Key;
    }
}