using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.PlatformServices.GameController
{
    public interface IGameControllerService : INotifyCollectionChanged
    {
        event EventHandler<GameControllerEventArgs> GameControllerEvent;

        bool IsControllerIdSupported { get; }

        bool TryGetController(string id, [MaybeNullWhen(false)] out IGameController controller);
    }
}
