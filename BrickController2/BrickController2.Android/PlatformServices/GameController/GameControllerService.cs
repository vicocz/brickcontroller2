using Android.Views;
using Android.Hardware.Input;
using Android.Content;
using BrickController2.PlatformServices.GameController;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace BrickController2.Droid.PlatformServices.GameController
{
    internal class GameControllerService : GameControllerServiceBase<int, InputDevice, GamepadController>
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
            if (TryGetGamepadDevice(deviceId, out var device))
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
            RemoveController(deviceId);
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is changed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        internal void MainActivityOnInputDeviceChanged(int deviceId)
        {
            if (TryGetGamepadDevice(deviceId, out var device))
            {
                // handle change
                AddGameControllerDevice(device);
            }
            else
            {
                // just for sure, remove it
                RemoveController(deviceId);
            }
        }

        internal bool OnGameControllerButtonEvent(KeyEvent e, float buttonValue)
        {
            if (!TryGetActiveController(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            return gamepadController.OnButtonEvent(e, buttonValue);
        }

        internal bool OnGameControllerAxisEvent(MotionEvent e)
        {
            if (!TryGetActiveController(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            return gamepadController.OnAxisEvent(e);
        }

        protected override void InitializeCurrentControllers()
        {
            ClearGameControllers();

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

        protected override void RemoveAllControllers()
        {
            ClearGameControllers();
        }

        /// <summary>
        /// Remove any registered game controller
        /// </summary>
        private void ClearGameControllers()
        {
            foreach (int deviceId in AllControllerKeys)
            {
                RemoveController(deviceId);
            }
        }

        /// <summary>
        /// Add game controller device represented by native instance of <paramref name="gamepad"/>
        /// </summary>
        private void AddGameControllerDevice(InputDevice gamepad)
        {
            lock (_lockObject)
            {
                // handle update
                if (TryGetActiveController(gamepad.Id, out var currentController))
                {
                    if (currentController.ControllerNumber != gamepad.ControllerNumber)
                    {
                        _logger.LogDebug("Gampad {deviceId} has changed.", gamepad.Id);
                    }

                    // ignore it as e.g. ControllerNumber has changed
                    return;
                }

                var newController = new GamepadController(this, gamepad);

                AddController(gamepad.Id, newController);
            }
        }

        private static bool TryGetGamepadDevice(int deviceId, [MaybeNullWhen(false)] out InputDevice device)
        {
            device = InputDevice.GetDevice(deviceId);

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