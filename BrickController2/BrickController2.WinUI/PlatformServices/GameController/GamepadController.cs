using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.Extensions;
using Microsoft.Maui.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GamepadController
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private readonly GameControllerService _controllerService;
    private readonly Gamepad _gamepad;
    private readonly IDispatcherTimer _timer;

    private readonly Dictionary<(GameControllerEventType, string), float> _lastReadingValues = [];

    public GamepadController(GameControllerService service, Gamepad gamepad, IDispatcherTimer timer)
        : this(service, gamepad, timer, DefaultInterval)
    {
    }

    private GamepadController(GameControllerService service, Gamepad gamepad, IDispatcherTimer timer, TimeSpan timerInterval)
    {
        _controllerService = service;
        _gamepad = gamepad;
        _timer = timer;

        _timer.Interval = timerInterval;
        _timer.Tick += Timer_Tick;
    }

    public string DeviceId => _gamepad.GetDeviceId();

    public void Start()
    {
        _lastReadingValues.Clear();

        // finally start timer
        _timer.Start();
    }

    public void Stop(bool sendResetEvent = false)
    {
        _timer.Stop();

        if (sendResetEvent)
        {
            var currentEvents = _lastReadingValues
                .Where(x => x.Value != 0)
                .ToDictionary(x => x.Key, x => GamepadReadingExtensions.Zero);

            _controllerService.RaiseEvent(currentEvents);
        }

        _lastReadingValues.Clear();
    }

    private void Timer_Tick(object? sender, object e)
    {
        var currentReading = _gamepad.GetCurrentReading();

        var currentEvents = currentReading
            .Enumerate()
            .Where(HasChanged)
            .ToDictionary(x => (x.EventType, x.Name), x => x.Value);

        _controllerService.RaiseEvent(currentEvents);
    }

    private static bool AreAlmostEqual(float a, float b) => Math.Abs(a - b) < 0.001;

    private bool HasChanged((string AxisName, GameControllerEventType EventType, float Value) readingValue)
    {
        var eventKey = (readingValue.EventType, readingValue.AxisName); 
        // get last reported value of the default one
        _lastReadingValues.TryGetValue(eventKey, out float lastValue);
        // skip value if there is no change
        if (AreAlmostEqual(readingValue.Value, lastValue))
        {
            return false;
        }

        _lastReadingValues[eventKey] = readingValue.Value;
        return true;
    }
}
