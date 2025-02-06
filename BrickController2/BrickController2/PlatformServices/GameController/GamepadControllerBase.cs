using System.Collections.Generic;
using System.Linq;
using static BrickController2.PlatformServices.GameController.GameController;

namespace BrickController2.PlatformServices.GameController;

public abstract class GamepadControllerBase<TGamepad>
{
    private readonly Dictionary<string, float> _lastAxisValues = [];

    protected GamepadControllerBase(GameControllerServiceBase controllerService,
        TGamepad gamepad,
        int controllerIndex,
        string persistenceId)
    {
        ControllerService = controllerService;
        Gamepad = gamepad;
        ControllerIndex = controllerIndex;
        ControllerId = GetControllerIdFromIndex(controllerIndex);
        UniquePersistantDeviceId = persistenceId;
    }

    /// <summary>
    /// Reference to GameControllerService
    /// </summary>
    public GameControllerServiceBase ControllerService { get; }

    /// <summary>
    /// Unique and persistant identifier of device
    /// </summary>
    public TGamepad Gamepad { get; }

    /// <summary>
    /// Index of this controller inside the controller management
    /// </summary>
    public int ControllerIndex { get; }

    /// <summary>
    /// string to identify the controller like "Controller 1"
    /// </summary>
    public string ControllerId { get; }

    /// <summary>
    /// Unique and persistant identifier of device
    /// </summary>
    public string UniquePersistantDeviceId { get; }

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
        ControllerService.RaiseEvent(new GameControllerEventArgs(ControllerId, events));
    }

    protected void RaiseEvent(GameControllerEventType eventType, string eventCode, float value)
    {
        ControllerService.RaiseEvent(new GameControllerEventArgs(ControllerId, eventType, eventCode, value));
    }
}
