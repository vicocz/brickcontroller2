using System.Collections.Generic;
using System.Linq;
using static BrickController2.PlatformServices.GameController.GameControllers;

namespace BrickController2.PlatformServices.GameController;

public abstract class GamepadControllerBase<TGamepad> : IGameController where TGamepad : class
{
    /// <summary>stored last value per axis to detect changes</summary>
    private readonly Dictionary<string, float> _lastAxisValues = [];

    /// <summary>Controller service that owns/manages the controller</summary>
    private readonly IGameControllerServiceInternal _controllerService;

    protected GamepadControllerBase(IGameControllerServiceInternal controllerService,
        TGamepad gamepad)
    {
        _controllerService = controllerService;
        Gamepad = gamepad;
    }

    /// <summary>
    /// Index of this controller inside the controller management
    /// </summary>
    public int ControllerNumber { get; protected init; }

    /// <summary>
    /// string to identify the controller like "Controller 1"
    /// </summary>
    public string ControllerId { get; protected init; } = default!;

    /// <summary>
    /// Unique and persistant identifier of device
    /// </summary>
    public string UniquePersistantDeviceId { get; protected init; } = default!;

    public string Name { get; protected init; } = default!;

    public int VendorId { get; protected init; }
    public int ProductId { get; protected init; }

    /// <summary>
    /// Native instance of gamepad
    /// </summary>
    public TGamepad Gamepad { get; }

    public virtual void Start()
    {
        // initialize
        _lastAxisValues.Clear();
    }

    public virtual void Stop()
    {
        // reset last values
        _lastAxisValues.Clear();
    }

    protected bool ContainsAxisValue(string axisName) => _lastAxisValues.ContainsKey(axisName);

    protected bool HasValueChanged(string axisName, float value)
    {
        // get last reported value or the default one
        _lastAxisValues.TryGetValue(axisName, out float lastValue);
        // skip value if there is no change
        if (AreAlmostEqual(value, lastValue))
        {
            return false;
        }
        // persist
        _lastAxisValues[axisName] = value;
        return true;
    }

    protected void RaiseEvent(IDictionary<(GameControllerEventType, string), float> events)
    {
        if (!events.Any())
        {
            return;
        }
        _controllerService.RaiseEvent(new GameControllerEventArgs(ControllerId, events));
    }

    protected void RaiseEvent(GameControllerEventType eventType, string eventCode, float value)
    {
        _controllerService.RaiseEvent(new GameControllerEventArgs(ControllerId, eventType, eventCode, value));
    }
}
