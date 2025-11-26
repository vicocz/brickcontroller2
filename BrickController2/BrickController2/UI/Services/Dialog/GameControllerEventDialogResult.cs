using BrickController2.PlatformServices.InputDevice;

namespace BrickController2.UI.Services.Dialog
{
    public class GameControllerEventDialogResult
    {
        public GameControllerEventDialogResult(bool isOk, InputDeviceEventType eventType, string eventCode)
            : this(isOk, string.Empty, eventType, eventCode)
        {
        }

        public GameControllerEventDialogResult(bool isOk, string controllerId, InputDeviceEventType eventType, string eventCode)
        {
            IsOk = isOk;
            EventType = eventType;
            EventCode = eventCode;
            ControllerId = controllerId;
        }

        public bool IsOk { get; }
        public InputDeviceEventType EventType { get; }
        public string EventCode { get; }
        public string ControllerId { get; }
    }
}
