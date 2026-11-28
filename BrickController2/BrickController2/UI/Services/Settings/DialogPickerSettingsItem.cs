// Base class of picker rows that open a dialog; owns the cancellation token that closes the dialog when the page goes away.
using System;
using System.Threading;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// A picker row whose press action shows dialogs. The dialogs are cancelled when the settings page disappears
/// (replaces the page view model's DisappearingToken, same pattern as the USB stick rows).
/// </summary>
public abstract class DialogPickerSettingsItem : PickerSettingsItem
{
    private CancellationTokenSource? _attachedTokenSource;

    /// <summary>
    /// Creates the row.
    /// </summary>
    /// <param name="title">Already translated label.</param>
    /// <param name="getValue">Returns the text for the picker button.</param>
    protected DialogPickerSettingsItem(string title, Func<string> getValue)
        : base(title, getValue)
    {
    }

    /// <summary>
    /// Token that is cancelled when the page disappears; <c>default</c> (never cancelled) while not attached.
    /// </summary>
    protected CancellationToken DisappearingToken => _attachedTokenSource?.Token ?? default;

    /// <inheritdoc />
    public override void Attach()
    {
        // a fresh source per appearance, mirrors PageViewModelBase.OnAppearing
        _attachedTokenSource?.Cancel();
        _attachedTokenSource = new CancellationTokenSource();
    }

    /// <inheritdoc />
    public override void Detach()
    {
        _attachedTokenSource?.Cancel();
        _attachedTokenSource = null;
    }
}
