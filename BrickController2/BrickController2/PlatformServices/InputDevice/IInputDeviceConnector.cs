using System.Collections.Generic;

namespace BrickController2.PlatformServices.InputDevice;

internal interface IInputDeviceConnector
{
    internal bool HasValueChanged(InputDeviceEventType eventType, string eventCode, float value);

    internal void RaiseEvent(IDictionary<(InputDeviceEventType, string), float> events);
}
