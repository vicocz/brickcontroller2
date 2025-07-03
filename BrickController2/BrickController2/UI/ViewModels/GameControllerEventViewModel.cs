using BrickController2.PlatformServices.GameController;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels
{
    public class GameControllerEventViewModel : NotifyPropertyChangedSource
    {
        private float _value;

        public GameControllerEventViewModel(GameControllerEventKey eventKey, float value)
        {
            EventType = eventKey.EventType;
            EventCode = eventKey.EventCode;
            EventAlias = eventKey.EventAlias;
            Value = value;
        }

        public GameControllerEventType EventType { get; }
        public string EventCode { get; }

        public string? EventAlias { get; }

        public bool ContainsEventAlias => string.IsNullOrEmpty(EventAlias) == false;

        public float Value
        {
            get => _value;
            set { _value = value; RaisePropertyChanged(); }
        }

        public bool IsMatch(GameControllerEventKey other) =>
            EventType == other.EventType &&
            EventCode == other.EventCode &&
            EventAlias == other.EventAlias;
    }
}
