using System;
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Android.Views;
using Android.Hardware.Input;
using Android.Content;
using BrickController2.PlatformServices.GameController;
using Newtonsoft.Json;
using static Android.Hardware.Camera;
using static Android.Renderscripts.ScriptGroup;

namespace BrickController2.Droid.PlatformServices.GameController
{
    public class GameControllerService : IGameControllerService
    {
        private readonly Dictionary<int, GamepadController> _availableControllers = [];
        private readonly IDictionary<Axis, float> _lastAxisValues = new Dictionary<Axis, float>();
        private readonly object _lockObject = new object();
        private readonly InputManager _inputManager;

        private event EventHandler<GameControllerEventArgs>? GameControllerEventInternal;

        public GameControllerService(Context context)
        {
            _inputManager = (InputManager)context.GetSystemService(Context.InputService)!;
        }

        public event EventHandler<GameControllerEventArgs> GameControllerEvent
        {
            add
            {
                lock (_lockObject)
                {
                    if (GameControllerEventInternal == null)
                    {
                        _lastAxisValues.Clear();
                    }

                    GameControllerEventInternal += value;
                }
            }

            remove
            {
                lock (_lockObject)
                {
                    GameControllerEventInternal -= value;
                }
            }
        }

        public bool IsControllerIdSupported => true;


        /// <summary>
        /// Handler called from MainActivity when MainActivity is created
        /// </summary>
        public void MainActivityOnCreate()
        {
            // get all known gamecontrollers
            RefreshGameControllers();
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is added
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        public void MainActivityOnInputDeviceAdded(int deviceId)
        {
            AddGameControllerDevice(deviceId);
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is removed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        public void MainActivityOnInputDeviceRemoved(int deviceId)
        {
            RemoveGameControllerDevice(deviceId);
        }

        /// <summary>
        /// Handler called from MainActivity when an InputDevice is changed 
        /// </summary>
        /// <param name="deviceId">deviceId of InputDevice</param>
        public void MainActivityOnInputDeviceChanged(int deviceId)
        {
            AddGameControllerDevice(deviceId);
        }

        public bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (e.Source.HasFlag(InputSourceType.Gamepad) && 
                e.RepeatCount == 0 &&
                _availableControllers.TryGetValue(e.DeviceId, out GamepadController? gamepadController)) // fetch matching GamepadController from table
            {
                GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(gamepadController.ControllerId, GameControllerEventType.Button, e.KeyCode.ToString(), 1.0F));
                return true;
            }

            return false;
        }

        public bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (e.Source.HasFlag(InputSourceType.Gamepad) && 
                e.RepeatCount == 0 &&
                _availableControllers.TryGetValue(e.DeviceId, out GamepadController? gamepadController)) // fetch matching GamepadController from table
            {
                GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(gamepadController.ControllerId, GameControllerEventType.Button, e.KeyCode.ToString(), 0.0F));
                return true;
            }
            return false;
        }

        public bool OnGenericMotionEvent(MotionEvent e)
        {
            if (e.Source == InputSourceType.Joystick && 
                e.Action == MotionEventActions.Move &&
                _availableControllers.TryGetValue(e.DeviceId, out GamepadController? gamepadController)) // fetch matching GamepadController from table
            {
                var events = new Dictionary<(GameControllerEventType, string), float>();
                foreach (Axis axisCode in Enum.GetValues(typeof(Axis)))
                {
                    var axisValue = e.GetAxisValue(axisCode);

                    if ((axisCode == Axis.Rx || axisCode == Axis.Ry) && 
                        e.Device?.VendorId == 1356 && 
                        (e.Device?.ProductId == 2508 || e.Device?.ProductId == 1476))
                    {
                        // DualShock 4 hack for the triggers ([-1:1] -> [0:1])
                        if (!_lastAxisValues.ContainsKey(axisCode) && axisValue == 0.0F)
                        {
                            continue;
                        }

                        axisValue = (axisValue + 1) / 2;
                    }

                    if (e.Device?.VendorId == 0x057e && 
                        (/*e.Device.ProductId == 0x2006 || e.Device.ProductId == 0x2007 ||*/ e.Device.ProductId == 0x2009))
                    {
                        // Nintendo Switch Pro controller hack ([-0.69:0.7] -> [-1:1])
                        // 2006 and 2007 are for the Nintendo Joy-Con controller (haven't reported issues with it)
                        axisValue = Math.Min(1, Math.Max(-1, axisValue / 0.69F));
                    }

                    if (e.Device?.VendorId == 1118 && e.Device?.ProductId == 765 &&
                        axisCode == Axis.Generic1)
                    {
                        // XBox One controller reports a constant value on Generic 1 - filter it out
                        continue;
                    }

                    axisValue = AdjustControllerValue(axisValue);

                    if (_lastAxisValues.TryGetValue(axisCode, out float lastValue))
                    {
                        if (AreAlmostEqual(axisValue, lastValue))
                        {
                            // axisValue == lastValue
                            continue;
                        }
                    }

                    _lastAxisValues[axisCode] = axisValue;
                    events[(GameControllerEventType.Axis, axisCode.ToString())] = axisValue;
                }

                GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(gamepadController.ControllerId, events));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add any connected game controller
        /// </summary>
        private void RefreshGameControllers()
        {
            lock (_lockObject)
            {
                ClearGameControllers();

                int[] deviceIds = _inputManager?.GetInputDeviceIds() ?? Array.Empty<int>();

                foreach (int deviceId in deviceIds)
                {
                    AddGameControllerDevice(deviceId);
                }
            }
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
                if (!_availableControllers.ContainsKey(deviceId))
                {
                    InputDevice gamepad = InputDevice.GetDevice(deviceId)!;

                    if (gamepad?.Sources.HasFlag(InputSourceType.Gamepad) == true || // null-check included
                        gamepad?.Sources.HasFlag(InputSourceType.Joystick) == true)
                    {
                        if (gamepad?.Name?.StartsWith("uinput-") == true) // drop all gamepads with name starting with "uinput-"
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

                            GamepadController newController = new GamepadController(this, gamepad!, controllerIndex);

                            _availableControllers[deviceId] = newController;
                        }
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

        private static float AdjustControllerValue(float value)
        {
            value = Math.Abs(value) < 0.05 ? 0.0F : value;
            value = value > 0.95 ? 1.0F : value;
            value = value < -0.95 ? -1.0F : value;
            return value;
        }

        private static bool AreAlmostEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.001;
        }
    }
}