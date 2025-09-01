using System.Collections.Generic;
using System.Linq;
using BrickController2.iOS.PlatformServices.ModelContextProtocol;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.ModelContextProtocol;
using Foundation;
using GameController;
using Microsoft.Extensions.Logging;

namespace BrickController2.iOS.PlatformServices.GameController
{
    internal class GameControllerService : GameControllerServiceBase
    {
        private readonly McpServerService _mcpServerService;
        private NSObject? _didConnectNotification;
        private NSObject? _didDisconnectNotification;

        public GameControllerService(ILogger<GameControllerService> logger,
               McpServerService mcpServerService) : base(logger)
        {
            _mcpServerService = mcpServerService;
        }

        public override bool IsControllerIdSupported => true;

        protected override void InitializeCurrentControllers()
        {
            // add McpServer
            AddMcpServer();

            // get all available gamepads
            if (GCController.Controllers.Any())
            {
                AddDevices(GCController.Controllers);
            }

            // register GCController events
            _didDisconnectNotification = GCController.Notifications.ObserveDidDisconnect((sender, args) =>
            {
                var controller = args.Notification.Object as GCController;
                if (controller != null)
                {
                    ControllerRemoved(controller);
                }
            });
            _didConnectNotification = GCController.Notifications.ObserveDidConnect((sender, args) =>
            {
                var controller = args.Notification.Object as GCController;
                if (controller != null)
                {
                    ControllerAdded(controller);
                }
            });

            GCController.StartWirelessControllerDiscovery(() => { });

            _mcpServerService.McpServerAdded += McpServerAdded;
            _mcpServerService.McpServerRemoved += McpServerRemoved; ;
        }


        protected override void RemoveAllControllers()
        {
            GCController.StopWirelessControllerDiscovery();
            _didConnectNotification?.Dispose();
            _didDisconnectNotification?.Dispose();
            _didConnectNotification = null;
            _didDisconnectNotification = null;

            _mcpServerService.McpServerAdded -= McpServerAdded;
            _mcpServerService.McpServerRemoved -= McpServerRemoved; ;

            base.RemoveAllControllers();
        }

        private void ControllerRemoved(GCController controller)
        {
            lock (_lockObject)
            {
                if (TryRemove<GamepadController>(x => x.ControllerDevice == controller, out var controllerDevice))
                {
                    _logger.LogInformation("ControllerDevice has been removed ControllerId:{controllerId}", controllerDevice.ControllerId);
                }
            }
        }

        private void ControllerAdded(GCController controller)
        {
            AddDevices([controller]);
        }

        private void AddDevices(IEnumerable<GCController> controllers)
        {
            lock (_lockObject)
            {
                foreach (var gamepad in controllers)
                {
                    // get first unused number and apply it
                    int controllerNumber = GetFirstUnusedControllerNumber();
                    var newController = new GamepadController(this, gamepad, controllerNumber);

                    AddController(newController);
                }
            }
        }

        private void McpServerRemoved(object? sender, McpServer e)
        {
            lock (_lockObject)
            {
                if (TryRemove<McpServerController>(x => x.ControllerDevice is McpServer, out var controller))
                {
                    _logger.LogInformation("McpServer has been removed");
                }
            }
        }

        private void McpServerAdded(object? sender, McpServer e)
        {
            AddMcpServer();
        }

        private void AddMcpServer()
        {
            if (_mcpServerService?.Server != null)
            {
                lock (_lockObject)
                {
                    var newController = new McpServerController(this, _mcpServerService.Server);

                    AddController(newController);
                }
            }
        }
    }
}