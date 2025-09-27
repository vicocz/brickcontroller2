using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.PlatformServices.GameController
{
    public class GameControllerEventArgs : EventArgs
    {
        public GameControllerEventArgs(string controllerId, GameControllerEventType eventType, string eventCode, float value)
        {
            ControllerId = controllerId;
            ControllerEvents = new Dictionary<(GameControllerEventType, string), float>
            {
                [(eventType, eventCode)] = value
            };
        }

        public GameControllerEventArgs(string controllerId, IDictionary<(GameControllerEventType, string), float> events)
        {
            ControllerId = controllerId;
            ControllerEvents = new ReadOnlyDictionary<(GameControllerEventType, string), float>(events);
        }

        public IReadOnlyDictionary<(GameControllerEventType EventType, string EventCode), float> ControllerEvents { get; }
        public string ControllerId { get; }
    }
}
