using BrickController2.DeviceManagement;
using System;
using System.Text.RegularExpressions;

namespace BrickController2.UI.Services.Help;

/// <summary>
/// Identifies a help topic. The path is relative to the docs root, e.g. "devices/pfxbrick" or "pages/creation-list".
/// </summary>
public sealed record HelpTopic(string Path)
{
    private const string PageViewModelSuffix = "PageViewModel";

    private static readonly Regex CapitalLetter = new("(?<!^)([A-Z])", RegexOptions.Compiled);

    public static HelpTopic ForDevice(DeviceType deviceType)
        => new($"devices/{deviceType.ToString().ToLowerInvariant()}");

    /// <summary>
    /// Creates a topic for a page based on its view model type name (e.g. CreationListPageViewModel => pages/creation-list).
    /// </summary>
    public static HelpTopic ForPage(string viewModelTypeName)
    {
        var name = viewModelTypeName.EndsWith(PageViewModelSuffix, StringComparison.Ordinal)
            ? viewModelTypeName[..^PageViewModelSuffix.Length]
            : viewModelTypeName;

        return new($"pages/{CapitalLetter.Replace(name, "-$1").ToLowerInvariant()}");
    }
}
