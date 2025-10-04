using BrickController2.Extensions;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BrickController2.InputDeviceManagement;

/// <summary>
/// manages the lifecycle of inputdevice services (like gamecontroller service, MCP server service) 
/// and inputdevices (like gamecontrollers, MCP server) 
/// and raises inputdevice events (joystick movement, button press) to registered listeners
/// </summary>
public sealed class InputDeviceManagerService : IInputDeviceManagerService
{
    private readonly object _lockObject = new();
    private event EventHandler<InputDeviceEventArgs>? InputDeviceEventInternal;

    /// <summary>
    /// Collection of registered inputdevice services (i.e. gamecontroller service, MCP server) 
    /// (registering of inputdevice services is expected to be done during initialization and is not expected to be thread-safe)
    /// </summary>
    private readonly List<IInputDeviceService> _registeredInputDeviceServices = [];

    /// <summary>
    /// Collection of available inputdevices (i.e. gamecontrollers, MCP server) 
    /// </summary>
    private readonly List<IInputDevice> _availableInputDevices = [];

    /// <summary>
    /// inputdevice event (i.e. joystick movement, button press)
    /// </summary>
    public event EventHandler<InputDeviceEventArgs> InputDeviceEvent
    {
        add
        {
            lock (_lockObject)
            {
                if (InputDeviceEventInternal == null)
                {
                    // first listener -> initialize inputdevice services
                    StopInputDeviceServices();
                    InitializeInputDeviceServices();
                }

                InputDeviceEventInternal += value;
            }
        }

        remove
        {
            lock (_lockObject)
            {
                InputDeviceEventInternal -= value;

                if (InputDeviceEventInternal == null)
                {
                    // last listener removed -> stop inputdevice services
                    StopInputDeviceServices();
                }
            }
        }
    }

    /// <summary>
    /// Event raised when inputdevices are connected or disconnected
    /// </summary>
    public event EventHandler<InputDeviceChangedEventArgs>? InputDevicesChangedEvent;

    /// <summary>
    /// returns true if inputdevice events can be processed (i.e. if there is at least one listener)
    /// </summary>
    public bool CanProcessEvents
    {
        get
        {
            lock (_lockObject)
            {
                return InputDeviceEventInternal != null;
            }
        }
    }

    /// <summary>
    /// Register an inputdevice service (i.e. gamecontroller service, MCP server service)
    /// </summary>
    /// <param name="inputDeviceService">inputdevice service to be registered</param>
    public void RegisterInputDeviceService(IInputDeviceService inputDeviceService)
    {
        // registering of inputdevice services is expected to be done during initialization and is not expected to be thread-safe
        _registeredInputDeviceServices.Add(inputDeviceService);
    }

    public void RaiseEvent(InputDeviceEventArgs eventArgs)
    {
        InputDeviceEventInternal?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Initialize collection of available InputDeviceServices (including listening of connected/disconnected controller)
    /// </summary>
    private void InitializeInputDeviceServices()
    {
        _registeredInputDeviceServices.ForEach(service => service?.Initialize());
    }

    /// <summary>
    /// Stop all registered inputdevice services and remove and stop all available inputdevices
    /// </summary>
    public void StopInputDeviceServices()
    {
        _registeredInputDeviceServices.ForEach(service => service?.Stop());

        RemoveAllInputDevices();
    }

    /// <summary>
    /// add inputdevice to the manager
    /// </summary>
    /// <param name="inputdevice">inputdevice to be added</param>
    public void AddInputDevice(IInputDevice inputDevice)
    {
        lock (_lockObject)
        {
            _availableInputDevices.Add(inputDevice);
            inputDevice.Start();

            // notify adding
            RaiseInputDevicesChanged(NotifyInputDevicesChangedAction.Connected, inputDevice);
        }
    }

    /// <summary>
    /// try to remove inputdevice from the manager
    /// </summary>
    /// <typeparam name="TInputDevice">type of inputdevice</typeparam>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    public bool TryRemoveInputDevice<TInputDevice>(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
        where TInputDevice : class, IInputDevice
    {
        lock (_lockObject)
        {
            // remove and stop the controller
            if (_availableInputDevices.Remove(x => x is TInputDevice tc && predicate(tc), out var removed))
            {
                inputDevice = (TInputDevice)removed; // safe due to pattern match above
                inputDevice.Stop();

                // notify removal
                RaiseInputDevicesChanged(NotifyInputDevicesChangedAction.Disconnected, inputDevice);

                (inputDevice as IDisposable)?.Dispose();

                return true;
            }

            inputDevice = null;
            return false;
        }
    }

    /// <summary>
    /// try to get inputdevice from the manager
    /// </summary>
    /// <typeparam name="TInputDevice">type of inputdevice</typeparam>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    public bool TryGetInputDevice<TInputDevice>(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
        where TInputDevice : class, IInputDevice
    {
        lock (_lockObject)
        {
            inputDevice = _availableInputDevices.OfType<TInputDevice>().FirstOrDefault(x => predicate(x));
            return inputDevice is not null;
        }
    }

    /// <summary>
    /// Remove and stop all available inputdevices
    /// </summary>
    /// <remarks>this is expected to be called under the lock</remarks>
    private void RemoveAllInputDevices()
    {
        if (_availableInputDevices.Count == 0)
        {
            return;
        }

        var inputDevices = _availableInputDevices.ToArray();
        foreach (var inputDevice in _availableInputDevices)
        {
            inputDevice.Stop();

            (inputDevice as IDisposable)?.Dispose();
        }
        _availableInputDevices.Clear();

        // notify removal
        RaiseInputDevicesChanged(NotifyInputDevicesChangedAction.Disconnected, inputDevices);
    }

    /// <summary>
    /// raise inputdevices changed event
    /// </summary>
    private void RaiseInputDevicesChanged(NotifyInputDevicesChangedAction action, params IInputDevice[] inputDevices)
    {
        InputDevicesChangedEvent?.Invoke(this, new(action, inputDevices));
    }
}