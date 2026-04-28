using System;

namespace BrickController2.DeviceManagement.Lego;

/// <summary>
/// Immutable snapshot of a motor channel's position feedback.
/// </summary>
internal record struct ChannelPositionState(
    int AbsolutePosition,
    int RelativePosition,
    bool IsUpdated,
    DateTime UpdateTime)
{
    public static readonly ChannelPositionState Initial = new(
        AbsolutePosition: 0,
        RelativePosition: 0,
        IsUpdated: false,
        UpdateTime: DateTime.MinValue);

    /// <summary>Returns a new state with an updated relative position and timestamp.</summary>
    public ChannelPositionState WithRelativePosition(int relativePosition) =>
        this with { RelativePosition = relativePosition, IsUpdated = true, UpdateTime = DateTime.Now };

    /// <summary>Returns a new state with an updated absolute position.</summary>
    public ChannelPositionState WithAbsolutePosition(int absolutePosition) =>
        this with { AbsolutePosition = absolutePosition, IsUpdated = true, UpdateTime = DateTime.Now };

    /// <summary>Clears the IsUpdated flag after the update has been consumed.</summary>
    public ChannelPositionState ConsumeUpdate() =>
        this with { IsUpdated = false };
}
