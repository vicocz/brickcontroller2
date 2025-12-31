using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.InputDeviceManagement;

public interface IInputDeviceManagerService : IInputDeviceEventServiceInternal
{
    /// <summary>
    /// Register inputdevice service (i.e. gamecontroller service, MCP server service)
    /// </summary>
    /// <param name="inputDeviceService">inputdevice service to be registered</param>
    void RegisterInputDeviceService(IInputDeviceService inputDeviceService);

    /// <summary>
    /// returns true if inputdevice events can be processed (i.e. if there is at least one listener)
    /// </summary>
    bool CanProcessEvents { get; }

    /// <summary>
    /// add inputdevice to the manager
    /// </summary>
    /// <param name="inputDevice">inputdevice to be added</param>
    void AddInputDevice(IInputDevice inputDevice);

    /// <summary>
    /// try to get inputdevice from the manager
    /// </summary>
    /// <typeparam name="TInputDevice">type of inputdevice</typeparam>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    bool TryGetInputDevice<TInputDevice>(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
        where TInputDevice : class, IInputDevice;

    /// <summary>
    /// try to remove inputdevice from the manager
    /// </summary>
    /// <typeparam name="TInputDevice">type of inputdevice</typeparam>
    /// <param name="predicate">predicate to find inputdevice</param>
    /// <param name="inputDevice">inputdevice to be removed</param>
    /// <returns>True on success</returns>
    bool TryRemoveInputDevice<TInputDevice>(Predicate<TInputDevice> predicate, [MaybeNullWhen(false)] out TInputDevice inputDevice)
        where TInputDevice : class, IInputDevice;
}
