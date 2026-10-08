using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Help;

/// <summary>
/// Access to help documents embedded as docs/{topic path}/help[.culture].md
/// </summary>
internal static class HelpResources
{
    private const string Prefix = "docs/";

    private static readonly Lazy<Dictionary<string, string>> Resources = new(LoadResourceNames);

    public static bool Exists(HelpTopic topic) => Find(topic, CultureInfo.CurrentUICulture) is not null;

    public static string? Find(HelpTopic topic, CultureInfo culture)
    {
        foreach (var suffix in GetSuffixes(culture))
        {
            var key = $"{Prefix}{topic.Path}/help{suffix}.md";
            if (Resources.Value.TryGetValue(key, out var resourceName))
            {
                return resourceName;
            }
        }

        return null;
    }

    public static async Task<string?> ReadAsync(HelpTopic topic, CultureInfo culture)
    {
        var resourceName = Find(topic, culture);
        if (resourceName is null)
        {
            return null;
        }

        using var stream = typeof(HelpResources).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static IEnumerable<string> GetSuffixes(CultureInfo culture)
    {
        if (!string.IsNullOrEmpty(culture.Name))
        {
            yield return "." + culture.Name;
        }

        var language = culture.TwoLetterISOLanguageName;
        if (!string.IsNullOrEmpty(language) && language != "iv" && !string.Equals(language, culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            yield return "." + language;
        }

        yield return string.Empty;
    }

    private static Dictionary<string, string> LoadResourceNames()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in typeof(HelpResources).Assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) && name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                // logical names might contain backslashes depending on the build platform
                result[name.Replace('\\', '/')] = name;
            }
        }

        return result;
    }
}
