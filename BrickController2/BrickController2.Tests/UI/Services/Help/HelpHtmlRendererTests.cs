using BrickController2.UI.Services.Help;
using FluentAssertions;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace BrickController2.Tests.UI.Services.Help;

public class HelpHtmlRendererTests
{
    private static readonly HelpTopic Topic = new("devices/mk4");

    [Fact]
    public void Render_InlinesAppImage()
    {
        var html = HelpHtmlRenderer.Render("![MK](../../../../BrickController2/BrickController2/UI/Images/mk4_image.png)", Topic, Mock.Of<IHelpService>());

        html.Should().Contain("data:image/png;base64,");
    }

    [Fact]
    public void Render_KeepsUnknownImageUnchanged()
    {
        var html = HelpHtmlRenderer.Render("![x](images/unknown.png)", Topic, Mock.Of<IHelpService>());

        html.Should().Contain("images/unknown.png").And.NotContain("data:image");
    }

    [Fact]
    public void TopicImages_PointToExistingAppImages()
    {
        var docRoot = FindDocRoot();
        if (docRoot is null)
        {
            return; // repository sources are not available (e.g. packaged test run)
        }

        var image = new Regex(@"!\[[^\]]*\]\((?<url>[^)]+)\)");
        var invalid = Directory.EnumerateFiles(docRoot, "README.md", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}_template{Path.DirectorySeparatorChar}"))
            .SelectMany(file => image.Matches(File.ReadAllText(file)).Select(m => (file, url: m.Groups["url"].Value)))
            .Where(item => !File.Exists(Path.GetFullPath(item.url, Path.GetDirectoryName(item.file)!))
                || !item.url.Replace('\\', '/').Contains("UI/Images/", StringComparison.OrdinalIgnoreCase))
            .Select(item => $"{Path.GetRelativePath(docRoot, item.file)}: {item.url}")
            .ToList();

        invalid.Should().BeEmpty();
    }

    private static string? FindDocRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var doc = Path.Combine(dir.FullName, "doc");
            if (File.Exists(Path.Combine(doc, "README.md")))
            {
                return doc;
            }
        }

        return null;
    }
}
