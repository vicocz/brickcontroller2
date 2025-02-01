namespace BrickController2.PlatformServices.GameController;

using static BrickController2.PlatformServices.GameController.GameController;

public abstract class GamepadControllerBase<TGamepad>
{
    protected GamepadControllerBase(TGamepad gamepad, int controllerIndex, string persistenceId)
    {
        Gamepad = gamepad;
        ControllerIndex = controllerIndex;
        ControllerId = GetControllerIdFromIndex(controllerIndex);
        UniquePersistantDeviceId = persistenceId;
    }

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

    public abstract void Start();
    public abstract void Stop();
}
