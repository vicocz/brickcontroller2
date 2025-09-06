using BrickController2.PlatformServices.GameController;
using Foundation;
using GameController;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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
                if (args.Notification.Object is GCController controller)
                {
                    ControllerRemoved(controller);
                }
            });
            _didConnectNotification = GCController.Notifications.ObserveDidConnect((sender, args) =>
            {
                if (args.Notification.Object is GCController controller)
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
                    _logger.LogInformation("Controller device has been removed ControllerId:{controllerId}", controllerDevice.ControllerId);
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
                    // If PlayerIndex is unset then assign the next free player index
                    AssignNextAvailablePlayerIndex(gamepad);

                    // get first unused number and apply it
                    var newController = new GamepadController(this, gamepad);

                    AddController(newController);
                }
            }
        }

        /// <summary>
        /// If PlayerIndex is unset then assign the next free player index
        /// </summary>
        /// <param name="controller"></param>
        private void AssignNextAvailablePlayerIndex(GCController controller)
        {
            if (controller.PlayerIndex != GCControllerPlayerIndex.Unset)
            {
                return;
            }

            var usedIndexes = GCController.Controllers
                .Where(c => c.PlayerIndex != GCControllerPlayerIndex.Unset)
                .Select(c => c.PlayerIndex)
                .ToHashSet();

            foreach (GCControllerPlayerIndex index in Enum.GetValues(typeof(GCControllerPlayerIndex)))
            {
                if (index == GCControllerPlayerIndex.Unset) continue;
                if (!usedIndexes.Contains(index))
                {
                    controller.PlayerIndex = index;
                    break;
                }
            }
        }
    }
}