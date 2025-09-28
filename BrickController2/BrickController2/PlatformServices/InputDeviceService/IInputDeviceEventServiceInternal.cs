using BrickController2.PlatformServices.InputDevice;

namespace BrickController2.PlatformServices.InputDeviceService;

/// <summary>
/// internal interface to raise inputdevice events
/// </summary>
public interface IInputDeviceEventServiceInternal : IInputDeviceEventService
{
    /// <summary>
    /// raise inputdevice event
    /// </summary>
    /// <param name="eventArgs"></param>
    internal void RaiseEvent(InputDeviceEventArgs eventArgs);
}
