using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;
using Microsoft.Maui.Dispatching;
using BrickController2.Helpers;
using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.Extensions;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GamepadController
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private readonly GameControllerService _controllerService;
    private readonly Gamepad _gamepad;
    private readonly IDispatcherTimer _timer;

    private readonly Dictionary<string, float> _lastReadingValues = [];

    /// <summary>
    /// zero-based Index of this controller inside the controller management
    /// </summary>
    private readonly int _controllerIndex;

    /// <summary>
    /// string to identify the controller like "Controller 1"
    /// </summary>
    private readonly string _controllerId;

    /// <summary>
    /// Unique and persistant identifier of device (for future usage i.e. to save some device specific settings)
    /// this value won't change even if the input device is disconnected, reconnected, or reconfigured
    /// </summary>
    private readonly string _uniquePersistentDeviceId;

    public GamepadController(GameControllerService service, Gamepad gamepad, int controllerIndex, IDispatcherTimer timer)
        : this(service, gamepad, controllerIndex, timer, DefaultInterval)
    {
    }

    private GamepadController(GameControllerService service, Gamepad gamepad, int controllerIndex, IDispatcherTimer timer, TimeSpan timerInterval)
    {
        _controllerService = service;
        _gamepad = gamepad;
        _timer = timer;
        _controllerIndex = controllerIndex;
        _uniquePersistentDeviceId = _gamepad.GetUniquePersistentDeviceId();
        _controllerId = GameControllerHelper.GetControllerIdFromIndex(controllerIndex);

        _timer.Interval = timerInterval;
        _timer.Tick += Timer_Tick;
    }

    /// <summary>
    /// Unique and persistant identifier of device
    /// </summary>
    public string UniquePersistentDeviceId => _uniquePersistentDeviceId;

    /// <summary>
    /// Index of this controller inside the controller management
    /// </summary>
    public int ControllerIndex => _controllerIndex;

    /// <summary>
    /// string to identify the controller like "Controller 1"
    /// </summary>
    public string ControllerID => _controllerId;

    public Gamepad Gamepad => _gamepad;


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

    private void Timer_Tick(object? sender, object e)
    {
        var currentReading = _gamepad.GetCurrentReading();

        var currentEvents = currentReading
            .Enumerate()
            .Where(HasChanged)
            .ToDictionary(x => (x.EventType, x.Name), x => x.Value);

        _controllerService.RaiseEvent(currentEvents, ControllerID);
    }

    private static bool AreAlmostEqual(float a, float b) => Math.Abs(a - b) < 0.001;

    private bool HasChanged((string AxisName, GameControllerEventType EventType, float Value) readingValue)
    {
        // get last reported value of the default one
        _lastReadingValues.TryGetValue(readingValue.AxisName, out float lastValue);
        // skip value if there is no change
        if (AreAlmostEqual(readingValue.Value, lastValue))
        {
            return false;
        }

        _lastReadingValues[readingValue.AxisName] = readingValue.Value;
        return true;
    }
}
