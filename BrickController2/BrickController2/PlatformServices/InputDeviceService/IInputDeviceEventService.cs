using BrickController2.PlatformServices.InputDevice;
using System;

namespace BrickController2.PlatformServices.InputDeviceService;

/// <summary>
/// interface for services that provide inputdevice events (i.e. gamecontroller service, MCP server service)
/// </summary>
public interface IInputDeviceEventService
{
    /// <summary>
    /// Event raised when a inputdevice changes a value
    /// </summary>
    event EventHandler<InputDeviceEventArgs> InputDeviceEvent;

    /// <summary>
    /// Event raised when inputdevices are connected or disconnected
    /// </summary>
    event EventHandler<InputDeviceChangedEventArgs> InputDevicesChangedEvent;
}
