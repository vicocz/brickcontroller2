using System.Collections.Generic;
using BrickController2.DeviceManagement;

namespace BrickController2.UI.Images;

/// <summary>
/// Holds image and small image resource names for a device type.
/// </summary>
public record DeviceImageInfo(string ImageResourceName, string SmallImageResourceName);

/// <summary>
/// Registry for device type image resource mappings.
/// </summary>
public interface IDeviceImageRegistry
{
    /// <summary>
    /// Register image resource names for a device type.
    /// </summary>
    void Register(DeviceType deviceType, string imageResourceName, string smallImageResourceName);

    /// <summary>
    /// Get image info for a device type. Falls back to convention-based naming if not explicitly registered.
    /// </summary>
    DeviceImageInfo GetImages(DeviceType deviceType);
}

/// <summary>
/// Default implementation with convention-based fallback:
/// {devicetype}_image.png / {devicetype}_image_small.png
/// </summary>
public class DeviceImageRegistry : IDeviceImageRegistry
{
    private readonly Dictionary<DeviceType, DeviceImageInfo> _registry = [];

    public void Register(DeviceType deviceType, string imageResourceName, string smallImageResourceName)
    {
        _registry[deviceType] = new DeviceImageInfo(imageResourceName, smallImageResourceName);
    }

    public DeviceImageInfo GetImages(DeviceType deviceType)
    {
        if (!_registry.TryGetValue(deviceType, out var info))
        {
            // Convention-based fallback
            var typeName = deviceType.ToString().ToLowerInvariant();
            info = new DeviceImageInfo($"{typeName}_image.png", $"{typeName}_image_small.png");
            // Cache the convention-based result for future calls
            _registry[deviceType] = info;
        }
        return info;
    }
}
