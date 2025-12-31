using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDeviceService;
using Foundation;
using GameController;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.iOS.PlatformServices.GameController
{
    internal class GameControllerService : InputDeviceServiceBase<GamepadController>
    {
        private static readonly GCControllerPlayerIndex[] ValidPlayerIndexes =
            Enum.GetValues<GCControllerPlayerIndex>()
                .Except([GCControllerPlayerIndex.Unset])
                .ToArray();

        private NSObject? _didConnectNotification;
        private NSObject? _didDisconnectNotification;

        public GameControllerService(IInputDeviceManagerService inputDeviceManagerService, 
            ILogger<GameControllerService> logger) 
            : base(inputDeviceManagerService, logger)
        {
        }

        public override void Initialize()
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


        public override void Stop()
        {
            GCController.StopWirelessControllerDiscovery();
            _didConnectNotification?.Dispose();
            _didDisconnectNotification?.Dispose();
            _didConnectNotification = null;
            _didDisconnectNotification = null;
        }

        private void ControllerRemoved(GCController controller)
        {
            lock (_lockObject)
            {
                if (TryRemoveInputDevice(x => x.InputDeviceDevice == controller, out var controllerDevice))
                {
                    _logger.LogInformation("Controller device has been removed InputDeviceId:{controllerId}", controllerDevice.InputDeviceId);
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
                foreach (var controller in controllers)
                {
                    // If PlayerIndex is unset then assign the next free player index
                    AssignNextAvailablePlayerIndex(controller);

                    // get first unused number and apply it
                    var newController = new GamepadController(InputDeviceEventService, controller);

                    AddInputDevice(newController);
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
                return;

            var usedIndexes = GCController.Controllers
                .Where(c => c.PlayerIndex != GCControllerPlayerIndex.Unset)
                .Select(c => c.PlayerIndex)
                .ToHashSet();

            foreach (var index in ValidPlayerIndexes)
            {
                if (!usedIndexes.Contains(index))
                {
                    controller.PlayerIndex = index;
                    break;
                }
            }
        }
    }
}