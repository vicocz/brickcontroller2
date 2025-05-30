using Android.Views;
using Android.Hardware.Input;
using Android.Content;
using BrickController2.PlatformServices.GameController;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace BrickController2.Droid.PlatformServices.GameController
{
    internal class GameControllerService : GameControllerServiceBase<GamepadController>
    {
        private readonly InputManager _inputManager;

        public GameControllerService(Context context, ILogger<GameControllerService> logger) :base(logger)
        {
            _inputManager = (InputManager)context.GetSystemService(Context.InputService)!;
        }

        public override bool IsControllerIdSupported => true;

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is added
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        internal void MainActivityOnInputDeviceAdded(int deviceId)
        {
            if (CanProcessEvents && TryGetGamepadDevice(deviceId, out var device))
            {
                AddGameControllerDevice(device);
            }
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is removed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        internal void MainActivityOnInputDeviceRemoved(int deviceId)
        {
            if (TryRemove(x => x.Gamepad.Id == deviceId, out var controller))
            {
                _logger.LogInformation("Gamepad has been removed DeviceId:{id}, ControllerId:{controllerId}",
                    deviceId, controller.ControllerId);
            }
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is changed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        internal void MainActivityOnInputDeviceChanged(int deviceId)
        {
            var device = InputDevice.GetDevice(deviceId);
            if (device is not null)
            {
                if (CanProcessEvents && IsGamapadDevice(device))
                {
                    // if there is no existing controller present - add it
                    if (TryGetControllerByDeviceId(deviceId, out var controller) &&
                        controller.ControllerNumber == device.ControllerNumber)
                    {
                        // ignore it, it's some update
                        return;
                    }
                    else if (controller != null)
                    {
                        // handle change - remove and then add it again
                        TryRemove(x => x.Gamepad.Id == deviceId, out _);
                    }
                    AddGameControllerDevice(device);
                }
            }
            else if (TryRemove(x => x.Gamepad.Id == deviceId, out var controller))
            {
                _logger.LogInformation("Gamepad has been removed DeviceId:{id}, ControllerId:{controllerId}",
                    deviceId, controller.ControllerId);
            }
        }

        internal bool OnGameControllerButtonEvent(KeyEvent e, float buttonValue)
        {
            if (!TryGetControllerByDeviceId(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            return gamepadController.OnButtonEvent(e, buttonValue);
        }

        internal bool OnGameControllerAxisEvent(MotionEvent e)
        {
            if (!TryGetControllerByDeviceId(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            return gamepadController.OnAxisEvent(e);
        }

        protected override void InitializeCurrentControllers()
        {
            // add any connected game controller
            var deviceIds = _inputManager?.GetInputDeviceIds() ?? [];
            foreach (int deviceId in deviceIds)
            {
                if (TryGetGamepadDevice(deviceId, out var device))
                {
                    AddGameControllerDevice(device);
                }
            }
        }

        /// <summary>
        /// Add game controller device represented by native instance of <paramref name="gamepad"/>
        /// </summary>
        private void AddGameControllerDevice(InputDevice gamepad)
        {
            lock (_lockObject)
            {
                var newController = new GamepadController(this, gamepad);
                AddController(newController);
            }
        }

        private bool TryGetControllerByDeviceId(int deviceId, [MaybeNullWhen(false)] out GamepadController controller)
            => TryGetController(x => x.Gamepad.Id == deviceId, out controller);

        private static bool TryGetGamepadDevice(int deviceId, [MaybeNullWhen(false)] out InputDevice device)
        {
            device = InputDevice.GetDevice(deviceId);
            return IsGamapadDevice(device);
        }

        private static bool IsGamapadDevice(InputDevice? device)
        {
            // skip if device is missing or is strange one present
            if (device is null || device.Name?.StartsWith("uinput-") == true) // drop all gamepads with name starting with "uinput-"
            {
                // JK: Bug - Device 0 already taken by fingerprint reader on Android
                // https://github.com/godotengine/godot/issues/47656
                //
                // Input name       | Company Name
                // uinput-fpc       | Fingerprint Cards AB
                // uinput-goodix    | Goodix
                // uinput-synaptics | Synaptics
                // uinput-elan      | ElanTech
                // uinput-vfs       | Validity Sensors(acquired by Synaptics)
                // uinput-atrus     | Atrua Technologies
                return false;
            }

            // All input devices which are not gamepads or joysticks will be assigned a controller number of 0.
            return device.ControllerNumber > 0 &&
                (device.Sources.IsButtonEventSource() || device.Sources.IsAxisEventSource());
        }
    }
}