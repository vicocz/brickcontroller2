// Chooses the XAML row template of the settings page depending on the kind of settings item.
using BrickController2.UI.Services.Settings;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Pages;

/// <summary>
/// Picks the toggle template for <see cref="ISettingsToggleItem"/> and the picker template for every other item.
/// </summary>
public class SettingsItemTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Template of a picker row (label, button, optional status line).
    /// </summary>
    public DataTemplate? PickerTemplate { get; set; }

    /// <summary>
    /// Template of a toggle row (label and switch).
    /// </summary>
    public DataTemplate? ToggleTemplate { get; set; }

    /// <inheritdoc />
    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        => item is ISettingsToggleItem ? ToggleTemplate : PickerTemplate;
}
