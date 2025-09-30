using System;

namespace BrickController2.PlatformServices.GameController
{
    public interface IGameControllerService
    {
        event EventHandler<GameControllerEventArgs> GameControllerEvent;

        /// <summary>
        /// Event raised when a game controller is connected / disconnected
        /// </summary>
        event EventHandler<GameControllersChangedEventArgs> GameControllersChangedEvent;

        bool IsControllerIdSupported { get; }
    }
}
