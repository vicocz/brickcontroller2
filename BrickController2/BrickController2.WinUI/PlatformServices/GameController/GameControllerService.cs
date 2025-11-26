using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Windows.Gaming.Input;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Services.MainThread;
using Microsoft.Extensions.Logging;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GameControllerService : InputDeviceServiceBase<GamepadController>
{
    private readonly IMainThreadService _mainThreadService;
    private readonly IDispatcherProvider _dispatcherProvider;

    public GameControllerService(IMainThreadService mainThreadService,
        IDispatcherProvider dispatcherProvider,
        IInputDeviceManagerService inputDeviceManagerService,
        ILogger<GameControllerService> logger) 
        : base(inputDeviceManagerService, logger)
    {
        _mainThreadService = mainThreadService;
        _dispatcherProvider = dispatcherProvider;
    }

    public override void Initialize()
    {
        // get all available gamepads
        if (Gamepad.Gamepads.Any())
        {
            AddDevices(Gamepad.Gamepads);
        }

        // register gamepad events
        Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded += Gamepad_GamepadAdded;
    }

    public override void Stop()
    {
        // cancel gamepad events
        Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded -= Gamepad_GamepadAdded;
    }

    private void Gamepad_GamepadRemoved(object? sender, Gamepad gamepad)
    {
        lock (_lockObject)
        {
            // ensure stopped in UI thread
            _ = _mainThreadService.RunOnMainThread(() =>
            {
                if (TryRemoveInputDevice(x => x.InputDeviceDevice == gamepad, out var controller))
                {
                    _logger.LogInformation("Controller device has been removed InputDeviceId:{controllerId}", controller.InputDeviceId);
                }
            });
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
                var rawController = RawGameController.FromGameController(gamepad);
                if (rawController == null)
                {
                    // this might be some orphan, hard to say
                    continue;
                }
                // get first unused number and apply it
                int controllerNumber = GetFirstUnusedInputDeviceNumber();
                var newController = new GamepadController(InputDeviceEventService, gamepad!, rawController, controllerNumber, dispatcher!.CreateTimer());

                // UniquePersistantDeviceId looks like "{wgi/nrid/]Xd\\h-M1mO]-il0l-4L\\-Gebf:^3->kBRhM-d4}\0"                
                AddInputDevice(newController);
            }
        }
    }

    /// <summary>
    /// get first unused inputdevice number (starts from 1)
    /// </summary>
    /// <returns>first unused inputdevice number (starts from 1)</returns>
    private int GetFirstUnusedInputDeviceNumber()
    {
        lock (_lockObject)
        {
            int unusedNumber = 1;
            while (TryGetInputDevice(inputDevice => inputDevice.InputDeviceNumber == unusedNumber, out _))
            {
                unusedNumber++;
            }
            return unusedNumber;
        }
    }
}