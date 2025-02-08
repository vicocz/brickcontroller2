using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Windows.Gaming.Input;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.MainThread;
using BrickController2.Windows.Extensions;
using Microsoft.Extensions.Logging;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GameControllerService : GameControllerServiceBase<string, Gamepad, GamepadController>, IGameControllerService
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

        Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded += Gamepad_GamepadAdded;
    }

    protected override void RemoveAllControllers()
    {
        Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded -= Gamepad_GamepadAdded;

        foreach (var deviceId in AllControllerKeys)
        {
            RemoveController(deviceId);
        }
    }

    private void Gamepad_GamepadRemoved(object? sender, Gamepad e)
    {
        lock (_lockObject)
        {
            // JK: UniquePersistentDeviceId is not available
            //var deviceId = e.GetUniquePersistentDeviceId();
            string deviceId = GetKey(e);

            if (deviceId is null || !TryGetActiveController(deviceId, out var _))
            {
                return;
            }

            // ensure stopped in UI thread
            _ = _mainThreadService.RunOnMainThread(() => RemoveController(deviceId));
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

                int controllerNumber = GetFirstUnusedControllerNumber(); // get first unused index
                var newController = new GamepadController(this, gamepad!, controllerNumber, dispatcher!.CreateTimer());
                
                AddController(uniquePersistentDeviceId, newController);
            }
        }
    }
}