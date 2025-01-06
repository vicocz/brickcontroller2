using BrickController2.PlatformServices.GameController;

namespace BrickController2.UI.Services.Dialog
{
    public class GameControllerEventDialogResult
    {
        public GameControllerEventDialogResult(bool isOk, GameControllerEventType eventType, string eventCode)
            : this(isOk, string.Empty, eventType, eventCode)
        {
        }

        public GameControllerEventDialogResult(bool isOk, string controllerId, GameControllerEventType eventType, string eventCode)
        {
            IsOk = isOk;
            EventType = eventType;
            EventCode = eventCode;
            ControllerId = controllerId;
        }

        public bool IsOk { get; }
        public GameControllerEventType EventType { get; }
        public string EventCode { get; }
        public string ControllerId { get; }
    }
}
