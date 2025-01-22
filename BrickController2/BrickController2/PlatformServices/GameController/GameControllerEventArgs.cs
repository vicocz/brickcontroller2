using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.PlatformServices.GameController
{
    public class GameControllerEventArgs : EventArgs
    {
        private readonly string _controllerId;

        public GameControllerEventArgs(string controllerId, GameControllerEventType eventType, string eventCode, float value)
        {
            _controllerId = controllerId;
            var events = new Dictionary<(GameControllerEventType, string), float>();
            events[(eventType, eventCode)] = value;
            ControllerEvents = events;
        }

        public GameControllerEventArgs(string controllerId, IDictionary<(GameControllerEventType, string), float> events)
        {
            _controllerId = controllerId;
            ControllerEvents = new ReadOnlyDictionary<(GameControllerEventType, string), float>(events);
        }

        public IReadOnlyDictionary<(GameControllerEventType EventType, string EventCode), float> ControllerEvents { get; }
        public string ControllerId => _controllerId;
    }
}
