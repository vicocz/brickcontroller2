using BrickController2.PlatformServices.InputDeviceService;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.PlatformServices.InputDevice;

/// <summary>
/// abstract base class for input devices
/// </summary>
/// <typeparam name="TInputDeviceDevice">Type of native instance of inputdevice device</typeparam>
public abstract class InputDeviceBase<TInputDeviceDevice> : IInputDevice, IInputDeviceConnector
    where TInputDeviceDevice : class
{
    /// <summary>stored last reported value per (event type, event code)</summary>
    private readonly ConcurrentDictionary<(InputDeviceEventType EventType, string EventCode), float> _lastValues = new();

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
        _lastValues.Clear();
    }

    /// <summary>
    /// stop the inputdevice and publishing of its events
    /// </summary>
    public virtual void Stop()
    {
        // report the default value for everything which has been left in a non default state 
        RaiseEventsWithNonDefaultValues();

        // reset last values
        _lastValues.Clear();
    }

    protected bool ContainsAxisValue(string axisName) => _lastValues.ContainsKey((InputDeviceEventType.Axis, axisName));

    protected bool HasAxisValueChanged(string axisName, float value) => HasValueChanged(InputDeviceEventType.Axis, axisName, value);

    public bool HasValueChanged(InputDeviceEventType eventType, string eventCode, float value)
    {
        // get last reported value or the default one
        _lastValues.TryGetValue((eventType, eventCode), out float lastValue);
        // skip value if there is no change
        if (AreAlmostEqual(value, lastValue))
        {
            return false;
        }
        // persist
        _lastValues[(eventType, eventCode)] = value;
        return true;
    }

    public void RaiseEvent(IDictionary<(InputDeviceEventType, string), float> events)
    {
        if (!events.Any())
        {
            return;
        }
        _inputDeviceManagerService.RaiseEvent(new InputDeviceEventArgs(InputDeviceId, events));
    }

    private void RaiseEventsWithNonDefaultValues()
    {
        var resetEvents = _lastValues
            .Where(pair => !AreAlmostEqual(pair.Value, GetDefaultValue(pair.Key.EventType)))
            .ToDictionary(pair => pair.Key, pair => GetDefaultValue(pair.Key.EventType));

        RaiseEvent(resetEvents);

        static float GetDefaultValue(InputDeviceEventType eventType) => eventType switch
        {
            InputDeviceEventType.Button => BUTTON_RELEASED,
            _ => AXIS_ZERO_VALUE
        };
    }
}
