using System;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.PlatformServices.GameController
{
    public interface IGameControllerService
    {
        event EventHandler<GameControllerEventArgs> GameControllerEvent;

        /// <summary>
        /// Event raised when a game controller is connected / disconnected
        /// </summary>
        event EventHandler<NotifyGameControllersChangedEventArgs> GameControllersChangedEvent;

        bool IsControllerIdSupported { get; }

        bool TryGetController(string id, [MaybeNullWhen(false)] out IGameController controller);
    }
}
