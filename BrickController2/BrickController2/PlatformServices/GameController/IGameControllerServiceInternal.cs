using System;

namespace BrickController2.PlatformServices.GameController;

public interface IGameControllerServiceInternal : IGameControllerService
{
    void RaiseEvent(GameControllerEventArgs eventArgs);
}
