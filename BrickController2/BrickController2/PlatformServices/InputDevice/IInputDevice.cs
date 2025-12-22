using System.Collections.Generic;

namespace BrickController2.PlatformServices.InputDevice;

public interface IInputDevice
{
    /// <summary>
    /// String to identify the inputdevice like "Controller 1"
    /// </summary>
    string InputDeviceId { get; }

    /// <summary>
    /// Get logical inputdevice number
    /// </summary>
    /// <remarks>Starts from 1</remarks>
    int InputDeviceNumber { get; }

    /// <summary>
    /// inputdevice name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Start the inputdevice and publishing of its events
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the inputdevice and publishing of its events
    /// </summary>
    void Stop();
}


public interface IInputDevice<TDevice> : IInputDevice
    where TDevice : class
{
    /// <summary>
    /// Instance of source input device
    /// </summary>
    TDevice SourceInputDevice { get; }

    internal bool HasValueChanged(string axisName, float value);

    internal void RaiseEvent(IDictionary<(InputDeviceEventType, string), float> events);
}
