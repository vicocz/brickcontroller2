using BrickController2.DeviceManagement;
using BrickController2.UI.Images;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.UI.Converters;

public abstract class DeviceTypeToImageConverterBase
{
    private readonly Lazy<IDeviceImageRegistry> _registry = new(() => IPlatformApplication.Current!.Services.GetRequiredService<IDeviceImageRegistry>());

    protected bool TryGetImage(DeviceType deviceType, [NotNullWhen(true)] out DeviceImageInfo? imageInfo)
    {
        if (deviceType != DeviceType.Unknown)
        {
            imageInfo = _registry.Value.GetImages(deviceType);
            return true;
        }
        imageInfo = null;
        return false;
    }
}
