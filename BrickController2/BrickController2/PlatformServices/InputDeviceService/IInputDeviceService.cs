using BrickController2.PlatformServices.InputDevice;

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

public interface IInputDeviceService<TInputDevice> : IInputDeviceService
    where TInputDevice : class, IInputDevice
{
    /// <summary>
    /// Gets a value whether the input device is supported on this device.  
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets or sets a value whether the input device is enabled.
    /// </summary>
    bool IsEnabled { get; set; }
}
