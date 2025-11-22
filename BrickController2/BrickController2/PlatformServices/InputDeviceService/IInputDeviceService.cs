namespace BrickController2.PlatformServices.InputDeviceService;

public interface IInputDeviceService
{
    /// <summary>
    /// Initialize inputdevice service.
    /// Add collection of available controllers (including listening of connected/disconnected controller)
    /// </summary>
    void Initialize();

    /// <summary>
    /// Stop inputdevice service.
    /// </summary>
    void Stop();
}
