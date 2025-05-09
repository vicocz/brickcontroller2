using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BrickController2.Tests.UI.Images;

public class DeviceImagesResouceTests
{
    public static IEnumerable<object[]> DeviceTypesData =>
        Enum.GetValues(typeof(DeviceType))
            .Cast<DeviceType>()
            .Where(deviceType => deviceType != DeviceType.Unknown) // Exclude Unknown
            .Select(deviceType => new object[] { deviceType });

    [Theory]
    [MemberData(nameof(DeviceTypesData))]
    public void GetImageResource_DeviceType_SmallImageExists(DeviceType deviceType)
    {
        var smallImageName = $"{deviceType.ToString().ToLower()}" + "_image_small.png";

        AssertImageResourceExists(smallImageName);
    }

    [Theory]
    [MemberData(nameof(DeviceTypesData))]
    public void GetImageResource_DeviceType_ImageExists(DeviceType deviceType)
    {
        var smallImageName = $"{deviceType.ToString().ToLower()}" + "_image.png";

        AssertImageResourceExists(smallImageName);
    }

    private static void AssertImageResourceExists(string immageName)
    {
        var fullResourceName = ResourceHelper.GetImageResourcePath(immageName);

        var resourceNames = typeof(ResourceHelper).Assembly.GetManifestResourceNames();
        var exists = resourceNames.Contains(fullResourceName);

        exists.Should().BeTrue();
    }
}
