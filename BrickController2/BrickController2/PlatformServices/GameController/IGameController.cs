namespace BrickController2.PlatformServices.GameController;

public interface IGameController
{
    /// <summary>
    /// String to identify the controller like "Controller 1"
    /// </summary>
    string ControllerId { get; }

    /// <summary>
    /// Get logical controller number
    /// </summary>
    /// <remarks>Starts from 1</remarks>
    int ControllerNumber { get; }

    /// <summary>
    /// Controller name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Vendor ID of the game controller.
    /// </summary>
    int VendorId { get; }

    /// <summary>
    /// Product ID of the game controller.
    /// </summary>
    int ProductId { get; }

    /// <summary>
    /// Start the controller and publishing of its events
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the controller and publishing of its events
    /// </summary>
    void Stop();
}
