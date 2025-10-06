using Autofac;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.PlatformServices.InputDeviceService;

/// <summary>
/// abstract base class for inputdevice services (i.e. gamecontroller service, MCP server service)
/// </summary>
public abstract class InputDeviceServiceBase : IInputDeviceService,
    IStartable // ensure it's started as soon as the container is built in Autofac
{
    private readonly IInputDeviceManagerService _inputDeviceManagerService;
    protected readonly object _lockObject = new();
    protected readonly ILogger _logger;

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

    protected bool CanProcessEvents => _inputDeviceManagerService.CanProcessEvents;
    protected IInputDeviceEventServiceInternal InputDeviceEventService => _inputDeviceManagerService;

    /// <summary>
    /// add inputdevice to inputdevicemanager service
    /// </summary>
    /// <param name="inputDevice">inputdevice to be added</param>
    protected void AddInputDevice(IInputDevice inputDevice)
    {
        _inputDeviceManagerService.AddInputDevice(inputDevice);
    }

    /// <summary>
    /// try to remove inputdevice from the manager
    /// </summary>
    /// <typeparam name="TInputDevice">type of inputdevice</typeparam>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    protected bool TryRemoveInputDevice<TInputDevice>(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
        where TInputDevice : class, IInputDevice
    {
        return _inputDeviceManagerService.TryRemoveInputDevice(predicate, out inputDevice);
    }

    /// <summary>
    /// try to get inputdevice from the manager
    /// </summary>
    /// <typeparam name="TInputDevice">type of inputdevice</typeparam>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    protected bool TryGetInputDevice<TInputDevice>(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
        where TInputDevice : class, IInputDevice
    {
        return _inputDeviceManagerService.TryGetInputDevice(predicate, out inputDevice);
    }

    void IStartable.Start()
    {
    }
}