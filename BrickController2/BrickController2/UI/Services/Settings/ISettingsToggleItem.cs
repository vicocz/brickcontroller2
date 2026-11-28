// Toggle (switch) flavour of a settings row.
namespace BrickController2.UI.Services.Settings;

/// <summary>
/// A settings row that is rendered as label plus switch instead of a picker button.
/// <see cref="ISettingsItem.Value"/>, <see cref="ISettingsItem.Status"/> and <see cref="ISettingsItem.Command"/>
/// are not used by the toggle template.
/// </summary>
public interface ISettingsToggleItem : ISettingsItem
{
    /// <summary>
    /// Current state of the switch (two-way bound).
    /// </summary>
    bool IsToggled { get; set; }
}
