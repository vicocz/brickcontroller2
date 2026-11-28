// Ready-made toggle row: visibility and state access are supplied as delegates.
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.UI.Commands;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// A switch style settings row backed by delegates.
/// </summary>
public class ToggleSettingsItem : ISettingsToggleItem
{
    private readonly Func<bool> _isVisible;
    private readonly Func<bool> _get;
    private readonly Action<bool> _set;

    /// <summary>
    /// Creates the row.
    /// </summary>
    /// <param name="title">Already translated label.</param>
    /// <param name="isVisible">Returns whether the row is shown at all.</param>
    /// <param name="get">Reads the current switch state.</param>
    /// <param name="set">Writes a new switch state.</param>
    public ToggleSettingsItem(string title, Func<bool> isVisible, Func<bool> get, Action<bool> set)
    {
        Title = title;
        _isVisible = isVisible;
        _get = get;
        _set = set;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public bool IsVisible => _isVisible();

    /// <inheritdoc />
    public bool IsToggled
    {
        get => _get();
        set
        {
            // ignore no-op writes to avoid needless service calls
            if (_get() == value)
            {
                return;
            }

            _set(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsToggled)));
        }
    }

    /// <inheritdoc />
    public string Value => string.Empty;

    /// <inheritdoc />
    public string Status => string.Empty;

    /// <inheritdoc />
    public ICommand Command { get; } = new SafeCommand(() => Task.CompletedTask);

    /// <inheritdoc />
    public virtual int Order => 100;

    /// <inheritdoc />
    public void Attach()
    {
        // nothing to subscribe to
    }

    /// <inheritdoc />
    public void Detach()
    {
        // nothing to unsubscribe from
    }
}
