using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.MainThread;
using BrickController2.Windows.Extensions;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;

namespace BrickController2.Windows.PlatformServices.GameController
{
    public class GameControllerService : IGameControllerService
    {

        private readonly IDictionary<string, GamepadController> _availableControllers = new Dictionary<string, GamepadController>();
        private readonly object _lockObject = new object();
        private readonly IMainThreadService _mainThreadService;
        private CoreWindow _coreWindow;

        private event EventHandler<GameControllerEventArgs> GameControllerEventInternal;

        public GameControllerService(IMainThreadService mainThreadService)
        {
            _mainThreadService = mainThreadService;
        }

        public event EventHandler<GameControllerEventArgs> GameControllerEvent
        {
            add
            {
                lock (_lockObject)
                {
                    if (GameControllerEventInternal == null)
                    {
                        InitializeKeyHandling();
                        InitializeControllers();
                    }

                    GameControllerEventInternal += value;
                }
            }

            remove
            {
                lock (_lockObject)
                {
                    GameControllerEventInternal -= value;

                    if (GameControllerEventInternal == null)
                    {
                        TerminateKeyHandling();
                        TerminateControllers();
                    }
                }
            }
        }

        internal void RaiseEvent(IDictionary<(GameControllerEventType, string), float> events)
        {
            if (!events.Any())
            {
                return;
            }

            GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(events));
        }

        internal void RaiseEvent(string deviceId,string key, float value = 0.0f)
        {
            if (GameControllerEventInternal == null)
            {
                return;
            }

            GameControllerEventInternal.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Button, key, value));
        }

        internal void InitializeComponent(CoreWindow coreWindow)
        {
            _coreWindow = coreWindow;
        }

        internal bool OnKeyDown(KeyRoutedEventArgs e)
        {
            if (HandleKeyDown(e.DeviceId, e.OriginalKey, e.KeyStatus))
            {
                e.Handled = true;
                return true;
            }

            return false;
        }

        internal bool OnKeyUp(KeyRoutedEventArgs e)
        {
            if (HandleKeyUp(e.DeviceId, e.OriginalKey, e.KeyStatus))
            { 
                e.Handled = true;
                return true;
            }

            return false;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = HandleKeyDown(args.DeviceId, args.VirtualKey, args.KeyStatus);
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = HandleKeyUp(args.DeviceId, args.VirtualKey, args.KeyStatus);
        }

        private bool HandleKeyDown(string deviceId, VirtualKey key, CorePhysicalKeyStatus keyStatus)
        {
            if (GamepadMapping.IsGamepadButton(key, out string buttonCode, out float buttonValue))
            {
                if (keyStatus.RepeatCount == 1)
                {
                    RaiseEvent(deviceId, buttonCode, buttonValue);
                    return true;
                }
            }

            return false;
        }

        private bool HandleKeyUp(string deviceId, VirtualKey key, CorePhysicalKeyStatus keyStatus)
        {
            if (GamepadMapping.IsGamepadButton(key, out string buttonCode, out var _))
            {
                if (keyStatus.RepeatCount == 1)
                {
                    RaiseEvent(deviceId, buttonCode);
                    return true;
                }
            }

            return false;
        }

        private void InitializeKeyHandling()
        {
            if (_coreWindow == null)
                return;

            _coreWindow.KeyDown += CoreWindow_KeyDown;
            _coreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void TerminateKeyHandling()
        {
            if (_coreWindow == null)
                return;

            _coreWindow.KeyDown -= CoreWindow_KeyDown;
            _coreWindow.KeyUp -= CoreWindow_KeyUp;
        }

        private void InitializeControllers()
        {
            // get all available gamepads
            if (Gamepad.Gamepads.Any())
            {
                AddDevices(Gamepad.Gamepads);
            }

            Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
            Gamepad.GamepadAdded += Gamepad_GamepadAdded;
        }

        private void TerminateControllers()
        {
            Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
            Gamepad.GamepadAdded -= Gamepad_GamepadAdded;

            foreach (var controller in _availableControllers.Values)
            {
                controller.Stop();
            }
            _availableControllers.Clear();
        }

        private void Gamepad_GamepadRemoved(object sender, Gamepad e)
        {
            lock (_lockObject)
            {
                var deviceId = e.GetDeviceId();

                if (_availableControllers.TryGetValue(deviceId, out var controller))
                {
                    _availableControllers.Remove(deviceId);

                    // enesure created in UI thread
                    _ = _mainThreadService.RunOnMainThread(() => controller.Stop());
                }
            }
        }

        private void Gamepad_GamepadAdded(object sender, Gamepad e)
        {
            _ = AddDevicesInMainThread(new[] { e });
        }

        private Task AddDevicesInMainThread(IEnumerable<Gamepad> gamepads)
        {
            // enesure created in UI thread
            return _mainThreadService.RunOnMainThread(() => AddDevices(gamepads));
        }

        private void AddDevices(IEnumerable<Gamepad> gamepads)
        {
            lock (_lockObject)
            {
                foreach (var gamepad in gamepads)
                {
                    var deviceId = gamepad.GetDeviceId();

                    var newController = new GamepadController(this, gamepad);
                    _availableControllers[deviceId] = newController;

                    newController.Start();
                }
            }
        }
    }
}