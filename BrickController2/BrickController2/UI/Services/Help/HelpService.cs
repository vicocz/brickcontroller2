using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Help;

internal class HelpService : IHelpService
{
    public const string LinkScheme = "help://";

    private static readonly Regex TopicFile = new(@"^(?<topic>.+)/README(\.[\w-]+)?\.md$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool HasHelp(HelpTopic topic) => HelpResources.Exists(topic);

    public Task<string?> GetHelpMarkdownAsync(HelpTopic topic, CultureInfo? culture = null)
        => HelpResources.ReadAsync(topic, culture ?? CultureInfo.CurrentUICulture);

    public HelpTopic? ResolveLink(HelpTopic from, string href)
    {
        if (string.IsNullOrWhiteSpace(href) || href.Contains("://") || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var target = href.Split('#', '?')[0];
        if (!target.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // resolve relative to the folder of the current topic
        var baseUri = new Uri($"http://help/{from.Path}/");
        if (!Uri.TryCreate(baseUri, target, out var resolved))
        {
            return null;
        }

        var path = resolved.AbsolutePath.Trim('/');
        var match = TopicFile.Match(path);
        return match.Success ? new HelpTopic(match.Groups["topic"].Value) : null;
    }
}
