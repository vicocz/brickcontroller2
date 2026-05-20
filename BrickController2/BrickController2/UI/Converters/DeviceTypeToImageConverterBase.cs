using BrickController2.DeviceManagement;
using BrickController2.UI.Images;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrickController2.UI.Converters;

public abstract class DeviceTypeToImageConverterBase
{
    private readonly Lazy<IDeviceImageRegistry> _registry = new(() => IPlatformApplication.Current?.Services.GetRequiredService<IDeviceImageRegistry>()!);

    public DeviceImageInfo? GetImage(DeviceType deviceType)
    {
        if (deviceType != DeviceType.Unknown)
        {
            return _registry.Value.GetImages(deviceType);
        }
        return null;
    }
}
