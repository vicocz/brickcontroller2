namespace BrickController2.PlatformServices.GameController;

public interface IGameController
{
    string ControllerId { get; }
    int ControllerNumber { get; }

    string Name { get; }

    int VendorId { get; }
    int ProductId { get; }

    void Start();

    void Stop();
}
