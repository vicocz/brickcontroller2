using System;

namespace BrickController2.DeviceManagement.Lego;

/// <summary>
/// Immutable snapshot of a motor channel's position feedback.
/// </summary>
internal record struct ChannelPositionState(
    int Current,
    bool IsUpdated,
    DateTime UpdateTime)
{
    public static readonly ChannelPositionState Initial = new(
        Current: 0,
        IsUpdated: false,
        UpdateTime: DateTime.MinValue);

    /// <summary>Returns a new state with an updated position and timestamp.</summary>
    public ChannelPositionState WithPosition(int position) =>
        this with { Current = position, IsUpdated = true, UpdateTime = DateTime.Now };

    /// <summary>Clears the IsUpdated flag after the update has been consumed.</summary>
    public ChannelPositionState ConsumeUpdate() =>
        this with { IsUpdated = false };
}
