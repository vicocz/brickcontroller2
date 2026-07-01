using BrickController2.DeviceManagement;

namespace BrickController2.UI.Controls.Devices;

/// <summary>
/// Mirrors <see cref="BrickController2.DeviceManagement.IDeviceType{TDevice}"/> for the UI layer.
/// Provides a compile-time device-type identity for each channel selector view so that
/// the view registry and <see cref="DeviceChannelSelectorViewBase.RegisterChannelButtons"/>
/// can stamp buttons automatically without repeating the value per button in XAML.
/// </summary>
public interface IDeviceChannelSelectorView
{
    /// <summary>The <see cref="DeviceType"/> this view handles.</summary>
    static abstract DeviceManagement.DeviceType DeviceType { get; }
}
