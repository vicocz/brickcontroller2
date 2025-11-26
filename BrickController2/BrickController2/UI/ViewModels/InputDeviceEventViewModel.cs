using BrickController2.PlatformServices.InputDevice;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels;

public class InputDeviceEventViewModel : NotifyPropertyChangedSource
{
    private float _value;

    public InputDeviceEventViewModel(InputDeviceEventType eventType, string eventCode, float value)
    {
        EventType = eventType;
        EventCode = eventCode;
        Value = value;
    }

    public InputDeviceEventType EventType { get; }
    public string EventCode { get; }

    public float Value
    {
        get => _value;
        set { _value = value; RaisePropertyChanged(); }
    }
}
