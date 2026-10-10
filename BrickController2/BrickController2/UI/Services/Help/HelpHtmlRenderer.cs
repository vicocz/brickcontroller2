using BrickController2.Helpers;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BrickController2.UI.Services.Help;

internal static class HelpHtmlRenderer
{
    private static readonly Regex Heading = new(@"^#\s+(?<title>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

    private const string Style = """
        :root { color-scheme: light dark; }
        body { font-family: sans-serif; margin: 16px; line-height: 1.5; }
        h1 { font-size: 1.6em; } h2 { font-size: 1.3em; } h3 { font-size: 1.1em; }
        table { border-collapse: collapse; }
        th, td { border: 1px solid gray; padding: 4px 8px; text-align: left; }
        code, pre { font-family: monospace; background: rgba(128,128,128,0.2); }
        pre { padding: 8px; overflow-x: auto; }
        code { padding: 0 3px; }
        a { color: #4a90e2; }
        img { max-width: 100%; height: auto; }
        """;

    private const string LightStyle = ":root { color-scheme: light; } body { background: #ffffff; color: #000000; }";
    private const string DarkStyle = ":root { color-scheme: dark; } body { background: #121212; color: #e0e0e0; }";

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseAutoLinks()
        .Build();

    /// <summary>
    /// Renders markdown to a standalone HTML document. Links to other topics are rewritten to the help:// scheme.
    /// </summary>
    public static string Render(string markdown, HelpTopic topic, IHelpService helpService, bool isDark = false)
    {
        var document = Markdown.Parse(markdown, Pipeline);

        foreach (var link in document.Descendants<LinkInline>().Where(l => !l.IsImage && l.Url is not null).ToList())
        {
            var target = helpService.ResolveLink(topic, link.Url!);
            if (target is not null)
            {
                link.Url = HelpService.LinkScheme + target.Path;
            }
        }

        foreach (var image in document.Descendants<LinkInline>().Where(l => l.IsImage && l.Url is not null).ToList())
        {
            var dataUri = GetImageDataUri(image.Url!);
            if (dataUri is not null)
            {
                image.Url = dataUri;
            }
        }

        var body = document.ToHtml(Pipeline);
        return $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><style>{Style}{(isDark ? DarkStyle : LightStyle)}</style></head><body>{body}</body></html>";
    }

    /// <summary>
    /// Gets the text of the first level 1 heading.
    /// </summary>
    public static string? GetTitle(string markdown)
    {
        var match = Heading.Match(markdown);
        return match.Success ? match.Groups["title"].Value.Trim() : null;
    }

    /// <summary>
    /// Converts a link to an app image (.../UI/Images/{name}.png) to a data URI of the embedded resource.
    /// Only app images are supported, images in other locations are not embedded and stay unchanged.
    /// </summary>
    private static string? GetImageDataUri(string url)
    {
        const string ImagesFolder = "UI/Images/";
        var index = url.Replace('\\', '/').LastIndexOf(ImagesFolder, StringComparison.OrdinalIgnoreCase);
        if (index < 0 || !url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var resourceName = ResourceHelper.GetImageResourcePath(url[(index + ImagesFolder.Length)..]);
        using var stream = typeof(ResourceHelper).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return "data:image/png;base64," + Convert.ToBase64String(memory.ToArray());
    }
}
