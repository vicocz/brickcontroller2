using Autofac;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using Microsoft.Extensions.Logging;

namespace BrickController2.PlatformServices.InputDeviceService;

/// <summary>
/// abstract base class for inputdevice services (i.e. gamecontroller service, MCP server service)
/// </summary>
public abstract class InputDeviceServiceBase : IInputDeviceService,
    IStartable // ensure it's started as soon as the container is built in Autofac
{
    protected readonly object _lockObject = new();
    protected readonly ILogger _logger;
    protected readonly IInputDeviceManagerService _inputDeviceManagerService;

    protected InputDeviceServiceBase(IInputDeviceManagerService inputDeviceManagerService, ILogger logger)
    {
        _inputDeviceManagerService = inputDeviceManagerService;
        _logger = logger;

        // register to inputdevicemanager service
        _inputDeviceManagerService.RegisterInputDeviceService(this);
    }

    /// <summary>
    /// Initialize collection of available inputdevices (including listening of connected/disconnected controller)
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Remove and stop all available inputdevices
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// returns the first unused inputdevice number in inputdevice management
    /// </summary>
    protected int GetFirstUnusedInputDeviceNumber()
    {
        return _inputDeviceManagerService.GetFirstUnusedInputDeviceNumber();
    }

    /// <summary>
    /// add inputdevice to inputdevicemanager service
    /// </summary>
    /// <param name="inputDevice">inputdevice to be added</param>
    protected void AddInputDevice(IInputDevice inputDevice)
    {
        _inputDeviceManagerService.AddInputDevice(inputDevice);
    }

    void IStartable.Start()
    {
    }
}