using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.PlatformServices.InputDevice;

public class InputDeviceEventArgs : EventArgs
{
    public InputDeviceEventArgs(string inputDeviceId, InputDeviceEventType eventType, string eventCode, float value)
    {
        InputDeviceId = inputDeviceId;
        InputDeviceEvents = new Dictionary<(InputDeviceEventType, string), float>
        {
            [(eventType, eventCode)] = value
        };
    }

    public InputDeviceEventArgs(string inputDeviceId, IDictionary<(InputDeviceEventType, string), float> events)
    {
        InputDeviceId = inputDeviceId;
        InputDeviceEvents = new ReadOnlyDictionary<(InputDeviceEventType, string), float>(events);
    }

    public IReadOnlyDictionary<(InputDeviceEventType EventType, string EventCode), float> InputDeviceEvents { get; }
    public string InputDeviceId { get; }
}
