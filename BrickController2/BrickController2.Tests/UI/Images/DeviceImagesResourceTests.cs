using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Images;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BrickController2.Tests.UI.Images;

public class DeviceImagesResourceTests
{
    public static IEnumerable<object[]> DeviceTypesData =>
        Enum.GetValues<DeviceType>()
            .Where(deviceType => deviceType != DeviceType.Unknown) // Exclude Unknown
            .Select(deviceType => new object[] { deviceType });

    [Theory]
    [MemberData(nameof(DeviceTypesData))]
    public void GetImageResource_DeviceType_SmallImageExists(DeviceType deviceType)
    {
        var registry = CreateRegistry();
        var images = registry.GetImages(deviceType);

        AssertImageResourceExists(images.SmallImageResourceName);
    }

    [Theory]
    [MemberData(nameof(DeviceTypesData))]
    public void GetImageResource_DeviceType_ImageExists(DeviceType deviceType)
    {
        var registry = CreateRegistry();
        var images = registry.GetImages(deviceType);

        AssertImageResourceExists(images.ImageResourceName);
    }

    [Fact]
    public void Registry_ConventionFallback_ReturnsExpectedNames()
    {
        var registry = new DeviceImageRegistry();
        var images = registry.GetImages(DeviceType.SBrick);

        images.ImageResourceName.Should().Be("sbrick_image.png");
        images.SmallImageResourceName.Should().Be("sbrick_image_small.png");
    }

    [Fact]
    public void Registry_ExplicitRegistration_OverridesConvention()
    {
        var registry = new DeviceImageRegistry();
        registry.Register(DeviceType.SBrick, "custom_image.png", "custom_small.png");

        var images = registry.GetImages(DeviceType.SBrick);

        images.ImageResourceName.Should().Be("custom_image.png");
        images.SmallImageResourceName.Should().Be("custom_small.png");
    }

    private static void AssertImageResourceExists(string imageName)
    {
        var fullResourceName = ResourceHelper.GetImageResourcePath(imageName);

        var resourceNames = typeof(ResourceHelper).Assembly.GetManifestResourceNames();
        var exists = resourceNames.Contains(fullResourceName);

        exists.Should().BeTrue();
    }

    /// <summary>
    /// Creates a registry with the same non-convention mappings as DeviceManagementModule.
    /// </summary>
    private static DeviceImageRegistry CreateRegistry()
    {
        var registry = new DeviceImageRegistry();
        registry.Register(DeviceType.BuWizz2, "buwizz_image.png", "buwizz_image_small.png");
        registry.Register(DeviceType.RemoteControl, "remotecontrol_image_small.png", "remotecontrol_image_small.png");
        registry.Register(DeviceType.Infrared, "powerfunctions_image.png", "powerfunctions_image_small.png");
        return registry;
    }
}
