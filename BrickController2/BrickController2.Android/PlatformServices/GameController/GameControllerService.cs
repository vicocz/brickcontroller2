using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Hardware.Input;
using Android.Content;
using BrickController2.PlatformServices.GameController;

namespace BrickController2.Droid.PlatformServices.GameController
{
    public class GameControllerService : GameControllerServiceBase
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
            AddGameControllerDevice(deviceId);
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is removed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        internal void MainActivityOnInputDeviceRemoved(int deviceId)
        {
            RemoveGameControllerDevice(deviceId);
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is changed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        internal void MainActivityOnInputDeviceChanged(int deviceId)
        {
            AddGameControllerDevice(deviceId);
        }

        internal bool OnGameControllerButtonEvent(KeyEvent e, float buttonValue)
        {
            if (!_availableControllers.TryGetValue(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            RaiseEvent(e.KeyCode.ToString(), GameControllerEventType.Button, buttonValue, gamepadController.ControllerId);
            return true;
        }

        internal bool OnGameControllerAxisEvent(MotionEvent e)
        {
            if (!_availableControllers.TryGetValue(e.DeviceId, out GamepadController? gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            var events = gamepadController!.GetAxisEvents(e);
            RaiseEvent(events, gamepadController.ControllerId);
            return true;
        }

        protected override void InitializeControllers()
        {
            ClearGameControllers();

            // add any connected game controller
            var deviceIds = _inputManager?.GetInputDeviceIds() ?? [];
            foreach (int deviceId in deviceIds)
            {
                AddGameControllerDevice(deviceId);
            }
        }

        protected override void TerminateControllers()
        {
            ClearGameControllers();
        }

        /// <summary>
        /// Remove any registered game controller
        /// </summary>
        private void ClearGameControllers()
        {
            lock (_lockObject)
            {
                int[] savedKeys = _availableControllers.Keys.ToArray();

                foreach (int deviceId in savedKeys)
                {
                    RemoveGameControllerDevice(deviceId);
                }
            }
        }

        /// <summary>
        /// Add game controller device
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        private void AddGameControllerDevice(int deviceId)
        {
            lock (_lockObject)
            {
                if (_availableControllers.ContainsKey(deviceId))
                {
                    // skip if already present
                    return;
                }
                InputDevice gamepad = InputDevice.GetDevice(deviceId)!;

                if (gamepad is not null &&
                    (gamepad.Sources.IsButtonEventSource() || gamepad.Sources.IsAxisEventSource()))
                {
                    if (gamepad.Name?.StartsWith("uinput-") == true) // drop all gamepads with name starting with "uinput-"
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
                    }
                    else
                    {
                        int controllerIndex = GetFirstUnusedControllerIndex(); // get first unused index

                        var newController = new GamepadController(this, gamepad, controllerIndex);

                        _availableControllers[deviceId] = newController;
                    }
                }
            }
        }

        /// <summary>
        /// Remove game controller device
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        private void RemoveGameControllerDevice(int deviceId)
        {
            lock (_lockObject)
            {
                _availableControllers.Remove(deviceId);
            }
        }

        /// <summary>
        /// returns the first unused index of device in controller management
        /// </summary>
        /// <returns>first unused index</returns>
        private int GetFirstUnusedControllerIndex()
        {
            lock (_lockObject)
            {
                int unusedIndex = 0;
                while (_availableControllers.Values.Any(gamepadController => gamepadController.ControllerIndex == unusedIndex))
                {
                    unusedIndex++;
                }
                return unusedIndex;
            }
        }
    }
}