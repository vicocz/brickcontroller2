using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
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
        """;

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseAutoLinks()
        .Build();

    /// <summary>
    /// Renders markdown to a standalone HTML document. Links to other topics are rewritten to the help:// scheme.
    /// </summary>
    public static string Render(string markdown, HelpTopic topic, IHelpService helpService)
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

        var body = document.ToHtml(Pipeline);
        return $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><style>{Style}</style></head><body>{body}</body></html>";
    }

    /// <summary>
    /// Gets the text of the first level 1 heading.
    /// </summary>
    public static string? GetTitle(string markdown)
    {
        var match = Heading.Match(markdown);
        return match.Success ? match.Groups["title"].Value.Trim() : null;
    }
}
