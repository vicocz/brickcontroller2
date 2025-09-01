using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Windows.Gaming.Input;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.MainThread;
using Microsoft.Extensions.Logging;
using BrickController2.Windows.PlatformServices.ModelContextProtocol;
using BrickController2.PlatformServices.ModelContextProtocol;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GameControllerService : GameControllerServiceBase, IGameControllerService
{
    private readonly IMainThreadService _mainThreadService;
    private readonly IDispatcherProvider _dispatcherProvider;
    private readonly McpServerService _mcpServerService;

    public GameControllerService(IMainThreadService mainThreadService,
        IDispatcherProvider dispatcherProvider,
        McpServerService mcpServerService,
        ILogger<GameControllerService> logger) : base(logger)
    {
        _mainThreadService = mainThreadService;
        _dispatcherProvider = dispatcherProvider;
        _mcpServerService = mcpServerService;
    }

    public override bool IsControllerIdSupported => true;

    protected override void InitializeCurrentControllers()
    {
        // add McpServer
        AddMcpServer();

        // get all available gamepads
        if (Gamepad.Gamepads.Any())
        {
            AddDevices(Gamepad.Gamepads);
        }

        // register gamepad events
        Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded += Gamepad_GamepadAdded;

        _mcpServerService.McpServerAdded += McpServerAdded;
        _mcpServerService.McpServerRemoved += McpServerRemoved; ;
    }

    protected override void RemoveAllControllers()
    {
        // cancel gamepad events
        Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded -= Gamepad_GamepadAdded;

        _mcpServerService.McpServerAdded -= McpServerAdded;
        _mcpServerService.McpServerRemoved -= McpServerRemoved; ;

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
                if (TryRemove<GamepadController>(x => x.ControllerDevice == gamepad, out var controller))
                {
                    _logger.LogInformation("ControllerDevice has been removed ControllerId:{controllerId}", controller.ControllerId);
                }
            });
        }
    }

    private void Gamepad_GamepadAdded(object? sender, Gamepad e)
    {
        // ensure created in UI thread
        _ = _mainThreadService.RunOnMainThread(() => AddDevices([e]));
    }

    private void McpServerRemoved(object? sender, McpServer e)
    {
        lock (_lockObject)
        {
            // ensure stopped in UI thread
            _ = _mainThreadService.RunOnMainThread(() =>
            {
                if (TryRemove<McpServerController>(x => x.ControllerDevice is McpServer, out var controller))
                {
                    _logger.LogInformation("McpServer has been removed");
                }
            });
        }
    }

    private void McpServerAdded(object? sender, McpServer e)
    {
        // ensure created in UI thread
        _ = _mainThreadService.RunOnMainThread(() => AddMcpServer());
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
                int controllerNumber = GetFirstUnusedControllerNumber();
                var newController = new GamepadController(this, gamepad!, rawController, controllerNumber, dispatcher!.CreateTimer());

                // UniquePersistantDeviceId looks like "{wgi/nrid/]Xd\\h-M1mO]-il0l-4L\\-Gebf:^3->kBRhM-d4}\0"                
                AddController(newController);
            }
        }
    }

    private void AddMcpServer()
    {
        if (_mcpServerService?.Server != null)
        {
            lock (_lockObject)
            {
                var dispatcher = _dispatcherProvider.GetForCurrentThread();
                var newController = new McpServerController(this, _mcpServerService.Server, dispatcher);

                AddController(newController);
            }
        }
    }
}