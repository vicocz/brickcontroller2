using BrickController2.PlatformServices.GameController;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels
{
    public class GameControllerEventViewModel : NotifyPropertyChangedSource
    {
        private float _value;

        public GameControllerEventViewModel(string controllerId, IGameController? controller, GameControllerEventType eventType, string eventCode, float value)
        {
            ControllerId = controllerId;
            ControllerNumber = controller?.ControllerNumber ?? default;
            ControllerName = controller?.Name ?? "";
            EventType = eventType;
            EventCode = eventCode;
            Value = value;
        }

        public GameControllerEventType EventType { get; }
        public string EventCode { get; }
        public string ControllerId { get; }
        public int ControllerNumber { get; }
        public string ControllerName { get; }

        public float Value
        {
            get => _value;
            set { _value = value; RaisePropertyChanged(); }
        }
    }
}
