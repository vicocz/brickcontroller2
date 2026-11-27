// Generic model of one row of the settings page. The built-in rows (theme, language, orientation sensor)
// and rows contributed by a platform project all use this contract, so the page needs no hard-coded rows.
using System.ComponentModel;
using System.Windows.Input;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// One row of the settings page (picker style: label, button and optional status line).
/// Platforms may register further implementations in Autofac; platforms that register none show nothing extra.
/// </summary>
public interface ISettingsItem : INotifyPropertyChanged
{
    /// <summary>
    /// Label shown in the left column.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Text of the picker button shown in the right column.
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Small status line below the row; may be empty (the line is then hidden).
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Executed when the button is pressed (typically opens a selection dialog).
    /// </summary>
    ICommand Command { get; }

    /// <summary>
    /// Whether the row (including its divider) is shown. Defaults to visible so existing implementations keep working.
    /// </summary>
    bool IsVisible => true;

    /// <summary>
    /// Sort key of the row (ascending, ties keep registration order). The built-in rows use 0, 10 and 20;
    /// the default of 100 places platform rows after them, independent of the order in which Autofac modules were registered.
    /// </summary>
    int Order => 100;

    /// <summary>
    /// Called when the settings page appeared: subscribe to platform events and refresh.
    /// </summary>
    void Attach();

    /// <summary>
    /// Called when the settings page disappeared: unsubscribe and cancel open dialogs.
    /// </summary>
    void Detach();
}
