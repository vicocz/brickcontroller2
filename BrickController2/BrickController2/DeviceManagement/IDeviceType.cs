namespace BrickController2.DeviceManagement;

/// <summary>
/// Represents typed device <typeparamref name="TDevice"/>
/// </summary>
public interface IDeviceType<TDevice>
    where TDevice : Device
{
    /// <summary>
    /// Get type of the device.
    /// </summary>
    static abstract DeviceType Type { get; }

    /// <summary>
    /// Gets the human-readable name of the type.
    /// </summary>
    static abstract string TypeName { get; }
}
