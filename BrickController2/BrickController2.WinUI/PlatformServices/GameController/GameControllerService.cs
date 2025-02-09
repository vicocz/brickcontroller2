using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Windows.Gaming.Input;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.MainThread;
using Microsoft.Extensions.Logging;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GameControllerService : GameControllerServiceBase<GamepadController>, IGameControllerService
{
    private readonly IMainThreadService _mainThreadService;
    private readonly IDispatcherProvider _dispatcherProvider;

    public GameControllerService(IMainThreadService mainThreadService,
        IDispatcherProvider dispatcherProvider,
        ILogger<GameControllerService> logger) : base(logger)
    {
        _mainThreadService = mainThreadService;
        _dispatcherProvider = dispatcherProvider;
    }

    public override bool IsControllerIdSupported => true;

    protected override void InitializeCurrentControllers()
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

    protected override void RemoveAllControllers()
    {
        // cancel gamepad events
        Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded -= Gamepad_GamepadAdded;

        // do removal
        base.RemoveAllControllers();
    }

    private void Gamepad_GamepadRemoved(object? sender, Gamepad gamepad)
    {
        lock (_lockObject)
        {
            // ensure stopped in UI thread
            _ = _mainThreadService.RunOnMainThread(() =>
            {
                if (TryRemove(x => x.Gamepad == gamepad, out var controller))
                {
                    _logger.LogInformation("Gamepad has been removed ControllerId:{controllerId}", controller.ControllerId);
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
                // get first unused number and apply it
                int controllerNumber = GetFirstUnusedControllerNumber();
                var newController = new GamepadController(this, gamepad!, controllerNumber, dispatcher!.CreateTimer());

                // deviceId looks like "{wgi/nrid/]Xd\\h-M1mO]-il0l-4L\\-Gebf:^3->kBRhM-d4}\0"
                if(string.IsNullOrEmpty(newController.UniquePersistantDeviceId))
                {
                    _logger.LogDebug("Gamepad {gamepad} was not configured due to missing UniquePersistantDeviceId.", newController.Name);
                    continue;
                }
                
                AddController(newController);
            }
        }
    }
}