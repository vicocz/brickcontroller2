using System.Collections.Generic;

namespace BrickController2.PlatformServices.InputDevice;

public interface IInputDeviceConnector
{
    internal bool HasValueChanged(InputDeviceEventType eventType, string eventCode, float value);

    internal void RaiseEvent(IReadOnlyDictionary<(InputDeviceEventType, string), float> events);
}
