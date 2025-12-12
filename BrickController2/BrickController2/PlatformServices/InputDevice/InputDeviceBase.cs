using BrickController2.PlatformServices.InputDeviceService;
using System.Collections.Generic;
using System.Linq;
using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.PlatformServices.InputDevice;

/// <summary>
/// abstract base class for input devices
/// </summary>
/// <typeparam name="TInputDeviceDevice">Type of native instance of inputdevice device</typeparam>
public abstract class InputDeviceBase<TInputDeviceDevice> : IInputDevice 
    where TInputDeviceDevice : class
{
    /// <summary>stored last value per axis to detect changes</summary>
    private readonly Dictionary<string, float> _lastAxisValues = [];

    /// <summary>inputdevicemanager service that owns/manages the inputdevice</summary>
    private readonly IInputDeviceEventServiceInternal _inputDeviceManagerService;

    protected InputDeviceBase(IInputDeviceEventServiceInternal inputDeviceManagerService,
        TInputDeviceDevice inputDeviceDevice)
    {
        _inputDeviceManagerService = inputDeviceManagerService;
        InputDeviceDevice = inputDeviceDevice;
    }

    /// <summary>
    /// Index of this inputdevice inside the inputdevice management
    /// </summary>
    public int InputDeviceNumber { get; protected init; }

    /// <summary>
    /// string to identify the inputdevice like "Controller 1"
    /// </summary>
    public string InputDeviceId { get; protected init; } = default!;

    /// <summary>
    /// DisplayName of the inputdevice
    /// </summary>
    public string Name { get; protected init; } = default!;


    /// <summary>
    /// Native instance of inputdevice device
    /// </summary>
    public TInputDeviceDevice InputDeviceDevice { get; }

    /// <summary>
    /// start the inputdevice and publishing of its events
    /// </summary>
    public virtual void Start()
    {
        // initialize
        _lastAxisValues.Clear();
    }

    /// <summary>
    /// stop the inputdevice and publishing of its events
    /// </summary>
    public virtual void Stop()
    {
        // reset last values
        _lastAxisValues.Clear();
    }

    protected bool ContainsAxisValue(string axisName) => _lastAxisValues.ContainsKey(axisName);

    protected internal bool HasValueChanged(string axisName, float value)
    {
        // get last reported value or the default one
        _lastAxisValues.TryGetValue(axisName, out float lastValue);
        // skip value if there is no change
        if (AreAlmostEqual(value, lastValue))
        {
            return false;
        }
        // persist
        _lastAxisValues[axisName] = value;
        return true;
    }

    protected internal void RaiseEvent(IDictionary<(InputDeviceEventType, string), float> events)
    {
        if (!events.Any())
        {
            return;
        }
        _inputDeviceManagerService.RaiseEvent(new InputDeviceEventArgs(InputDeviceId, events));
    }

    protected void RaiseEvent(InputDeviceEventType eventType, string eventCode, float value)
    {
        _inputDeviceManagerService.RaiseEvent(new InputDeviceEventArgs(InputDeviceId, eventType, eventCode, value));
    }
}
