using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;
using Microsoft.Maui.Dispatching;
using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.Extensions;

using static BrickController2.PlatformServices.GameController.GameController;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GamepadController : GamepadControllerBase<Gamepad>
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private readonly GameControllerService _controllerService;
    private readonly IDispatcherTimer _timer;

    private readonly Dictionary<string, float> _lastReadingValues = [];

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">reference to GameControllerService</param>
    /// <param name="gamePad"> reference to InputDevice</param>
    /// <param name="controllerIndex">zero-based Index of device inside the controller management</param>
    public GamepadController(GameControllerService service, Gamepad gamepad, int controllerIndex, string persistentId, IDispatcherTimer timer)
        : base(gamepad, controllerIndex, persistentId)
    {
        _controllerService = service;
        _timer = timer;

        _timer.Interval = DefaultInterval;
        _timer.Tick += Timer_Tick;
    }

    public override void Start()
    {
        _lastReadingValues.Clear();

        // finally start timer
        _timer.Start();
    }

    public override void Stop()
    {
        _timer.Stop();

        _lastReadingValues.Clear();
    }

    private void Timer_Tick(object? sender, object e)
    {
        var currentReading = Gamepad.GetCurrentReading();

        var currentEvents = currentReading
            .Enumerate()
            .Where(HasChanged)
            .ToDictionary(x => (x.EventType, x.Name), x => x.Value);

        _controllerService.RaiseEvent(currentEvents, ControllerId);
    }

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
