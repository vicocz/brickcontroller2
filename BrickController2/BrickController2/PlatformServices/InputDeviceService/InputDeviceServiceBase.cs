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
public abstract class InputDeviceServiceBase<TInputDevice> : IInputDeviceService,
    IStartable // ensure it's started as soon as the container is built in Autofac
    where TInputDevice : class, IInputDevice
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
    protected void AddInputDevice(TInputDevice inputDevice)
    {
        _inputDeviceManagerService.AddInputDevice(inputDevice);
    }

    /// <summary>
    /// try to remove inputdevice from the manager
    /// </summary>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    protected bool TryRemoveInputDevice(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
    {
        return _inputDeviceManagerService.TryRemoveInputDevice(predicate, out inputDevice);
    }

    /// <summary>
    /// try to get inputdevice from the manager
    /// </summary>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    protected bool TryGetInputDevice(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
    {
        return _inputDeviceManagerService.TryGetInputDevice(predicate, out inputDevice);
    }

    /// <summary>
    /// Get first unused input device number (starts from 1) based on <typeparamref name="TInputDevice"/> type
    /// </summary>
    /// <returns>First unused input device number in context of <typeparamref name="TInputDevice"/> (starts from 1)</returns>
    protected int GetFirstUnusedInputDeviceNumber()
    {
        lock (_lockObject)
        {
            int unusedNumber = 1;
            while (_inputDeviceManagerService.TryGetInputDevice<TInputDevice>(inputDevice => inputDevice.InputDeviceNumber == unusedNumber, out _))
            {
                unusedNumber++;
            }
            return unusedNumber;
        }
    }

    void IStartable.Start()
    {
    }
}