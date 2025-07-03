using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.PlatformServices.GameController
{
    public class GameControllerEventArgs : EventArgs
    {
        public GameControllerEventArgs(string controllerId, GameControllerEventType eventType, string eventCode, float value)
            : this(controllerId, eventType, eventCode, null, value)
        {
        }

        public GameControllerEventArgs(string controllerId, GameControllerEventType eventType, string eventCode, string? eventAlias, float value)
        {
            ControllerId = controllerId;
            ControllerEvents = new Dictionary<GameControllerEventKey, float>
            {
                [new GameControllerEventKey(eventType, eventCode, eventAlias)] = value
            };
        }

        public GameControllerEventArgs(string controllerId, IDictionary<GameControllerEventKey, float> events)
        {
            ControllerId = controllerId;
            ControllerEvents = new ReadOnlyDictionary<GameControllerEventKey, float>(events);
        }

        public IReadOnlyDictionary<GameControllerEventKey, float> ControllerEvents { get; }
        public string ControllerId { get; }
    }
}
