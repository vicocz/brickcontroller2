using Android.Views;
using Android.Hardware.Input;
using Android.Content;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace BrickController2.Droid.PlatformServices.GameController
{
    internal class GameControllerService : InputDeviceServiceBase<GamepadController>
    {
        private readonly InputManager _inputManager;
        private readonly global::Android.Bluetooth.BluetoothManager? _bluetoothManager;
        private readonly System.Collections.Generic.Dictionary<int, (string? Descriptor, string RuntimeId)> _connectionIds = new();

        public GameControllerService(Context context,
            IInputDeviceManagerService inputDeviceManagerService,
            ILogger<GameControllerService> logger) 
            : base(inputDeviceManagerService, logger)
        {
            _inputManager = (InputManager)context.GetSystemService(Context.InputService)!;
            _bluetoothManager = context.GetSystemService(Context.BluetoothService) as global::Android.Bluetooth.BluetoothManager;
        }

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
            lock (_lockObject) _connectionIds.Remove(deviceId);
            if (TryRemoveInputDevice(x => x.InputDeviceDevice.Id == deviceId, out var controller))
            {
                _logger.LogInformation("InputDeviceDevice has been removed DeviceId:{id}, InputDeviceId:{controllerId}",
                    deviceId, controller.InputDeviceId);
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
                        controller.InputDeviceNumber == device.ControllerNumber &&
                        controller.InputDeviceDevice.Descriptor == device.Descriptor)
                    {
                        // ignore it, it's some update
                        return;
                    }
                    else if (controller != null)
                    {
                        // handle change - remove and then add it again
                        TryRemoveInputDevice(x => x.InputDeviceDevice.Id == deviceId, out _);
                        lock (_lockObject) _connectionIds.Remove(deviceId);
                    }
                    AddGameControllerDevice(device);
                }
                else if (!IsGamapadDevice(device))
                {
                    MainActivityOnInputDeviceRemoved(deviceId);
                }
            }
            else if (TryRemoveInputDevice(x => x.InputDeviceDevice.Id == deviceId, out var controller))
            {
                lock (_lockObject) _connectionIds.Remove(deviceId);
                _logger.LogInformation("InputDeviceDevice has been removed DeviceId:{id}, InputDeviceId:{controllerId}",
                    deviceId, controller.InputDeviceId);
            }
        }

        internal bool OnGameControllerButtonEvent(KeyEvent e, bool isPressed)
        {
            if (!TryGetControllerByDeviceId(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            var eventName = e.KeyCode.ToString();
            gamepadController.RaiseButtonEventConditionally(eventName, isPressed);
            return true;
        }

        internal bool OnGameControllerAxisEvent(MotionEvent e)
        {
            if (!TryGetControllerByDeviceId(e.DeviceId, out var gamepadController)) // fetch matching GamepadController from table
            {
                return false;
            }

            // grab all changed axis event
            var events = gamepadController.GetAxisEvents(e);
            gamepadController.RaiseEvent(events);
            return true;
        }

        public override void Initialize()
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
        public override void Stop()
        {
        }

        /// <summary>
        /// Add game controller device represented by native instance of <paramref name="gamepad"/>
        /// </summary>
        private void AddGameControllerDevice(InputDevice gamepad)
        {
            lock (_lockObject)
            {
                if (TryGetControllerByDeviceId(gamepad.Id, out _)) return;
                if (!_connectionIds.TryGetValue(gamepad.Id, out var connection) || connection.Descriptor != gamepad.Descriptor)
                    _connectionIds[gamepad.Id] = connection = (gamepad.Descriptor, "android:session:" + System.Guid.NewGuid().ToString("N"));
                var newController = new GamepadController(InputDeviceEventService, gamepad, connection.RuntimeId, GetBluetoothAlias(gamepad));
                AddInputDevice(newController);
            }
        }

        private bool TryGetControllerByDeviceId(int deviceId, [MaybeNullWhen(false)] out GamepadController controller)
            => TryGetInputDevice(x => x.InputDeviceDevice.Id == deviceId, out controller);

        private string? GetBluetoothAlias(InputDevice gamepad)
        {
            try
            {
                var paired = _bluetoothManager?.Adapter?.BondedDevices;
                if (paired == null) return null;
                var names = new System.Collections.Generic.List<(string Address, string? Alias)>();
                foreach (var device in paired)
                    if (device.Address != null)
                        names.Add((device.Address, System.OperatingSystem.IsAndroidVersionAtLeast(30)
                            ? device.Alias ?? device.Name : device.Name));
                return AndroidBluetoothAliasMatcher.FindAlias(gamepad.Descriptor, gamepad.VendorId,
                    gamepad.ProductId, gamepad.Name, names);
            }
            catch (Java.Lang.SecurityException)
            {
                // Nearby devices permission may be denied/revoked. Input still works with its native name.
                return null;
            }
        }

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
