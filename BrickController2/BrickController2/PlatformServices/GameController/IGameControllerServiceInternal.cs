namespace BrickController2.PlatformServices.GameController;

public interface IGameControllerServiceInternal : IGameControllerService
{
    internal void RaiseEvent(GameControllerEventArgs eventArgs);
}
