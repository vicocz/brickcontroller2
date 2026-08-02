using BrickController2.DeviceManagement;
using System;

namespace BrickController2.UI.Controls.Devices;

/// <summary>
/// Provides a compile-time device-type identity for each channel selector view so that
/// </summary>
public interface IDeviceChannelSelectorView
{
    /// <summary>The <see cref="DeviceType"/> this view handles.</summary>
    static virtual DeviceType DeviceType => throw new InvalidOperationException();
}
