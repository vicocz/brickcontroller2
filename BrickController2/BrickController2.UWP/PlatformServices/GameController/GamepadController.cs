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
                .ToDictionary(x => (x.Item1, x.Item2), x => x.Item3);

            _controllerService.RaiseEvent(currentEvents);
        }

        private IEnumerable<(GameControllerEventType, string, float)> GetCurrentReadings()
        {
            var currentReading = UwpController.GetCurrentReading();

            yield return (GameControllerEventType.Axis, nameof(GamepadReading.LeftThumbstickX), currentReading.LeftThumbstickX.ToControllerValue());
            yield return (GameControllerEventType.Axis, nameof(GamepadReading.LeftThumbstickY), currentReading.LeftThumbstickY.ToControllerValue());
            yield return (GameControllerEventType.Axis, nameof(GamepadReading.LeftTrigger), currentReading.LeftTrigger.ToControllerValue()); ;
            yield return (GameControllerEventType.Axis, nameof(GamepadReading.RightThumbstickX), currentReading.RightThumbstickX.ToControllerValue());
            yield return (GameControllerEventType.Axis, nameof(GamepadReading.RightThumbstickY), currentReading.RightThumbstickY.ToControllerValue());
            yield return (GameControllerEventType.Axis, nameof(GamepadReading.RightTrigger), currentReading.RightTrigger.ToControllerValue());
        }

        private static bool AreAlmostEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.001;
        }

        private bool HasChanged((GameControllerEventType, string, float) readingValue)
        {
            if (_lastReadingValues.TryGetValue(readingValue.Item2, out float lastValue))
            {
                if (AreAlmostEqual(readingValue.Item3, lastValue))
                {
                    // axisValue == lastValue
                    return false;
                }
            }

            _lastReadingValues[readingValue.Item2] = readingValue.Item3;
            return true;
        }
    }
}
