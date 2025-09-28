using System;
using System.Linq;
using Windows.Gaming.Input;
using Microsoft.Maui.Dispatching;
using BrickController2.Windows.Extensions;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;

using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GamepadController : InputDeviceBase<Gamepad>
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private readonly IDispatcherTimer _timer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">reference to GameControllerService</param>
    /// <param name="gamepad">reference to UWP's Gamepad</param>
    /// <param name="controllerNumber">zero-based Index of device inside the controller management</param>
    public GamepadController(IInputDeviceEventServiceInternal service, Gamepad gamepad, RawGameController rawController, int controllerNumber, IDispatcherTimer timer)
        : base(service, gamepad)
    {
        Name = rawController.DisplayName;
        InputDeviceNumber = controllerNumber;
        InputDeviceId = GetControllerIdFromNumber(controllerNumber);

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
        var currentReading = InputDeviceDevice.GetCurrentReading();

        var currentEvents = currentReading
            .Enumerate()
            .Where(x => HasValueChanged(x.Name, x.Value))
            .ToDictionary(x => (x.EventType, x.Name), x => x.Value);

        RaiseEvent(currentEvents);
    }
}
