using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;
using Windows.UI.Xaml;

namespace BrickController2.Windows.PlatformServices.GameController
{
    internal class GamepadController
    {
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(20);

        private readonly GameControllerService _controllerService;
        private readonly IDictionary<string, float> _lastReadingValues = new Dictionary<string, float>();

        private DispatcherTimer _timer;

        public GamepadController(GameControllerService service, Gamepad gamepad) : this(service, gamepad, DefaultInterval)
        {
        }

        private GamepadController(GameControllerService service, Gamepad gamepad, TimeSpan timerInterval)
        {
            _controllerService = service ?? throw new ArgumentNullException(nameof(service));
            UwpController = gamepad ?? throw new ArgumentNullException(nameof(gamepad));

            _timer = new DispatcherTimer
            {
                Interval = timerInterval
            };
            _timer.Tick += Timer_Tick;
        }

        public Gamepad UwpController { get; }

        public string DeviceId => UwpController.GetDeviceId();

        public void Start()
        {
            _lastReadingValues.Clear();

            // finally start timer
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();

            _lastReadingValues.Clear();
        }

        private void Timer_Tick(object sender, object e)
        {
            var currentReading = GetCurrentReadings();

            var currentEvents = currentReading
                .Where(HasChanged)
                .ToDictionary(x => (GameControllerEventType.Axis, x.AxisName), x => x.Value);

            _controllerService.RaiseEvent(currentEvents);
        }

        private IEnumerable<(string AxisName, float Value)> GetCurrentReadings()
        {
            var currentReading = UwpController.GetCurrentReading();

            yield return GamepadMapping.GetAxisValue(GamepadMapping.XAxis, currentReading.LeftThumbstickX);
            yield return GamepadMapping.GetAxisValue(GamepadMapping.YAxis, currentReading.LeftThumbstickY);
            yield return GamepadMapping.GetAxisValue(GamepadMapping.BrakeAxis, currentReading.LeftTrigger);
            yield return GamepadMapping.GetAxisValue(GamepadMapping.ZAxis, currentReading.RightThumbstickX);
            yield return GamepadMapping.GetAxisValue(GamepadMapping.RzAxis, currentReading.RightThumbstickY);
            yield return GamepadMapping.GetAxisValue(GamepadMapping.GasAxis, currentReading.RightTrigger);
        }

        private static bool AreAlmostEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.001;
        }

        private bool HasChanged((string AxisName, float Value) readingValue)
        {
            if (_lastReadingValues.TryGetValue(readingValue.AxisName, out float lastValue))
            {
                if (AreAlmostEqual(readingValue.Value, lastValue))
                {
                    // axisValue == lastValue
                    return false;
                }
            }

            _lastReadingValues[readingValue.AxisName] = readingValue.Value;
            return true;
        }
    }
}
