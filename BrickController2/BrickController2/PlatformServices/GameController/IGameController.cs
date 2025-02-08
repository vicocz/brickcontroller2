namespace BrickController2.PlatformServices.GameController;

public interface IGameController
{
    int ControllerNumber { get; }

    string Name { get; }

    void Start();

    void Stop();
}
