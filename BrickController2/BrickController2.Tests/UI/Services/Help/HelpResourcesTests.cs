using BrickController2.UI.Services.Help;
using FluentAssertions;
using System.Globalization;
using Xunit;

namespace BrickController2.Tests.UI.Services.Help;

public class HelpResourcesTests
{
    private static readonly HelpTopic PfxBrick = new("devices/pfxbrick");

    [Theory]
    [InlineData("en")]
    [InlineData("de-AT")]
    [InlineData("hu")]
    [InlineData("fr-FR")]
    public void Find_FallsBackToEnglish_WhenTopicIsNotTranslated(string culture)
    {
        var resource = HelpResources.Find(PfxBrick, CultureInfo.GetCultureInfo(culture));

        resource.Should().NotBeNull();
        resource!.Replace('\\', '/').Should().Be("doc/en/devices/pfxbrick/README.md");
    }

    [Fact]
    public void Find_ReturnsNull_ForUnknownTopic()
    {
        HelpResources.Find(new HelpTopic("devices/unknown"), CultureInfo.GetCultureInfo("en")).Should().BeNull();
    }

    [Fact]
    public void Find_DoesNotEmbedIndexFilesAndTemplates()
    {
        HelpResources.Find(new HelpTopic(""), CultureInfo.GetCultureInfo("en")).Should().BeNull();
        HelpResources.Find(new HelpTopic("_template/device"), CultureInfo.GetCultureInfo("en")).Should().BeNull();
    }
}
