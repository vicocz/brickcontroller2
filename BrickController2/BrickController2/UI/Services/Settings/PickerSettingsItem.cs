// Ready-made picker row: the owner supplies the displayed value and the press action as delegates.
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.UI.Commands;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// A picker style settings row whose value and press action are provided by delegates.
/// </summary>
public class PickerSettingsItem : ISettingsItem
{
    private readonly Func<string> _getValue;
    private readonly Func<Task>? _onPressed;

    /// <summary>
    /// Creates the row.
    /// </summary>
    /// <param name="title">Already translated label.</param>
    /// <param name="getValue">Returns the text for the picker button.</param>
    /// <param name="onPressed">Executed when the button is pressed.</param>
    public PickerSettingsItem(string title, Func<string> getValue, Func<Task> onPressed)
    {
        Title = title;
        _getValue = getValue;
        _onPressed = onPressed;
        Command = new SafeCommand(OnPressedAsync);
    }

    /// <summary>
    /// For derived rows that implement the press action by overriding <see cref="OnPressedAsync"/>.
    /// </summary>
    /// <param name="title">Already translated label.</param>
    /// <param name="getValue">Returns the text for the picker button.</param>
    protected PickerSettingsItem(string title, Func<string> getValue)
        : this(title, getValue, null!)
    {
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public string Value => _getValue();

    /// <inheritdoc />
    public string Status => string.Empty;

    /// <inheritdoc />
    public ICommand Command { get; }

    /// <inheritdoc />
    public virtual int Order => 100;

    /// <inheritdoc />
    public virtual void Attach()
    {
        // nothing to subscribe to
    }

    /// <inheritdoc />
    public virtual void Detach()
    {
        // nothing to unsubscribe from
    }

    /// <summary>
    /// Tells the UI that the value text changed (call after the underlying setting was changed).
    /// </summary>
    public void RaiseValueChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

    /// <summary>
    /// Executed when the button is pressed; the default runs the delegate given to the constructor.
    /// </summary>
    protected virtual Task OnPressedAsync() => _onPressed?.Invoke() ?? Task.CompletedTask;
}
