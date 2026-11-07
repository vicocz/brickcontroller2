using System.Globalization;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Help;

public interface IHelpService
{
    /// <summary>
    /// Checks whether a help document exists for the topic (in any supported language).
    /// </summary>
    bool HasHelp(HelpTopic topic);

    /// <summary>
    /// Gets the markdown of a topic. Falls back from the UI culture folder (for example, doc/de-AT) to its language folder (doc/de), then to the English folder (doc/en).
    /// </summary>
    Task<string?> GetHelpMarkdownAsync(HelpTopic topic, CultureInfo? culture = null);

    /// <summary>
    /// Resolves a relative link used inside a help document to another topic.
    /// </summary>
    HelpTopic? ResolveLink(HelpTopic from, string href);
}
