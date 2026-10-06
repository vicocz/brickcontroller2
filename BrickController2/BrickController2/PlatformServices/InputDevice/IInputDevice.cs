namespace BrickController2.PlatformServices.InputDevice;

public interface IInputDevice
{
    /// <summary>Identity of this connection; empty when the provider cannot track connections safely.
    /// Never persist it as a physical device identity. Reusable mapping labels are not connection identities.</summary>
    string RuntimeId => string.Empty;

    /// <summary>Persistent identity when the platform supplies one.</summary>
    string? AssignmentId => null;

    /// <summary>
    /// String to identify the inputdevice like "Controller 1"
    /// </summary>
    string InputDeviceId { get; }

    /// <summary>
    /// Get logical inputdevice number
    /// </summary>
    /// <remarks>Starts from 1</remarks>
    int InputDeviceNumber { get; }

    /// <summary>
    /// inputdevice name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Start the inputdevice and publishing of its events
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the inputdevice and publishing of its events
    /// </summary>
    void Stop();
}
