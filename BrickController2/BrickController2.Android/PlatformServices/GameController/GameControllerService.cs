using System.Collections.Generic;
using Android.Views;
using Android.Hardware.Input;
using Android.Content;
using BrickController2.PlatformServices.GameController;

namespace BrickController2.Droid.PlatformServices.GameController
{
    internal class GameControllerService : GameControllerServiceBase<int, InputDevice, GamepadController>
    {
        private readonly Dictionary<int, GamepadController> _availableControllers = [];
        private readonly object _lockObject = new object();
        private readonly InputManager _inputManager;

        public GameControllerService(Context context)
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
            var device = InputDevice.GetDevice(deviceId);
            AddGameControllerDevice(device);
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
            var gamepad = InputDevice.GetDevice(deviceId);
            // if gamepad is not found, remove it
            if (gamepad is null)
            {
                RemoveController(deviceId);
            }
            else
            {
                AddGameControllerDevice(gamepad);
            }
        }

        internal bool OnGameControllerButtonEvent(KeyEvent e, float buttonValue)
        {
            if (!TryGetActiveController(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            RaiseEvent(e.KeyCode.ToString(), GameControllerEventType.Button, buttonValue, gamepadController.ControllerId);
            return true;
        }

        internal bool OnGameControllerAxisEvent(MotionEvent e)
        {
            if (!TryGetActiveController(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            var events = gamepadController.GetAxisEvents(e);
            RaiseEvent(events, gamepadController.ControllerId);
            return true;
        }

        protected override void InitializeCurrentControllers()
        {
            ClearGameControllers();

            // add any connected game controller
            var deviceIds = _inputManager?.GetInputDeviceIds() ?? [];
            foreach (int deviceId in deviceIds)
            {
                var device = InputDevice.GetDevice(deviceId);
                AddGameControllerDevice(device);
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
        private void AddGameControllerDevice(InputDevice? gamepad)
        {
            lock (_lockObject)
            {
                // skip if device is missing or is strange one present
                if (gamepad is null || gamepad.Name?.StartsWith("uinput-") == true) // drop all gamepads with name starting with "uinput-"
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
                    return;
                }

                // skip if device is already present
                if (_availableControllers.ContainsKey(gamepad.Id))
                {
                    return;
                }

                if (gamepad.Sources.IsButtonEventSource() || gamepad.Sources.IsAxisEventSource())
                {                    
                    int controllerIndex = GetFirstUnusedControllerIndex(); // get first unused index

                    var newController = new GamepadController(this, gamepad, controllerIndex);

                    AddController(gamepad.Id, newController);
                }
            }
        }
    }
}