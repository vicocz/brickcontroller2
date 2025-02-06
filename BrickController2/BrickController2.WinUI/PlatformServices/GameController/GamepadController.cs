using System;
using System.Linq;
using Windows.Gaming.Input;
using Microsoft.Maui.Dispatching;
using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.Extensions;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GamepadController : GamepadControllerBase<Gamepad>
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private readonly IDispatcherTimer _timer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">reference to GameControllerService</param>
    /// <param name="gamePad"> reference to InputDevice</param>
    /// <param name="controllerIndex">zero-based Index of device inside the controller management</param>
    public GamepadController(GameControllerService service, Gamepad gamepad, int controllerIndex, string persistentId, IDispatcherTimer timer)
        : base(service, gamepad, controllerIndex, persistentId)
    {
        _timer = timer;

        _timer.Interval = DefaultInterval;
        _timer.Tick += Timer_Tick;
    }

    public override void Start()
    {
        base.Start();

        // finally start timer
        _timer.Start();
    }

    public override void Stop()
    {
        _timer.Stop();

        base.Stop();
    }

    private void Timer_Tick(object? sender, object e)
    {
        var currentReading = Gamepad.GetCurrentReading();

        var currentEvents = currentReading
            .Enumerate()
            .Where(x => HasValueChanged(x.Name, x.Value))
            .ToDictionary(x => (x.EventType, x.Name), x => x.Value);

        RaiseEvent(currentEvents);
    }
}
