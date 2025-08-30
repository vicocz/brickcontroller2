using System.Collections.Generic;
using System.Linq;
using BrickController2.PlatformServices.GameController;
using Foundation;
using GameController;
using Microsoft.Extensions.Logging;

namespace BrickController2.iOS.PlatformServices.GameController
{
    internal class GameControllerService : GameControllerServiceBase
    {
        private NSObject? _didConnectNotification;
        private NSObject? _didDisconnectNotification;

        public GameControllerService(ILogger<GameControllerService> logger) 
            : base(logger)
        {
        }

        public override bool IsControllerIdSupported => true;

        protected override void InitializeCurrentControllers()
        {
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
        }


        protected override void RemoveAllControllers()
        {
            GCController.StopWirelessControllerDiscovery();
            _didConnectNotification?.Dispose();
            _didDisconnectNotification?.Dispose();
            _didConnectNotification = null;
            _didDisconnectNotification = null;

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
    }
}