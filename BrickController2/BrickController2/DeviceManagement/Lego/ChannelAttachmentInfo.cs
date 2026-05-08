using System;

namespace BrickController2.DeviceManagement.Lego;

/// <summary>
/// Snapshot of peripheral attachment for a port.
/// </summary>
internal record struct ChannelAttachmentInfo(
    ushort DeviceId,
    bool IsUpdated,
    DateTime UpdateTime)
{
    public static readonly ChannelAttachmentInfo Initial = new(
        DeviceId: 0,
        IsUpdated: false,
        UpdateTime: DateTime.MinValue);

    /// <summary>Returns a new state with an updated device ID and timestamp.</summary>
    public ChannelAttachmentInfo WithDevice(ushort deviceId) =>
        this with { DeviceId = deviceId, IsUpdated = true, UpdateTime = DateTime.Now };
}
