using System;
using System.Linq;
using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls.Devices;

/// <summary>
/// Generic, fallback channel selector for <see cref="DeviceManagement.DeviceType.None"/>.
/// Presents a single Picker whose items are the 1-based channel names ("1", "2", ...),
/// while internally mapping to/from the 0-based <see cref="SelectedChannel"/> index.
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class GenericChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.Unknown;
    protected override DeviceType SelectorDeviceType => DeviceType;

    private bool _suppressPickerSelectionChanged;

    public GenericChannelSelectorView()
    {
        InitializeComponent();
    }

    protected override void OnDeviceChanged(Device device)
    {
        base.OnDeviceChanged(device);

        var channelNames = Enumerable.Range(1, Math.Max(device.NumberOfChannels, 0))
            .Select(channelNumber => channelNumber.ToString())
            .ToList();

        _suppressPickerSelectionChanged = true;
        ChannelPicker.ItemsSource = channelNames;
        ChannelPicker.SelectedIndex = channelNames.Count > 0
            ? Math.Clamp(SelectedChannel, 0, channelNames.Count - 1)
            : -1;
        _suppressPickerSelectionChanged = false;
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        base.OnSelectedChannelChanged(channel);

        if (ChannelPicker.SelectedIndex == channel)
        {
            return;
        }

        _suppressPickerSelectionChanged = true;
        ChannelPicker.SelectedIndex = channel;
        _suppressPickerSelectionChanged = false;
    }

    private void OnChannelPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressPickerSelectionChanged || ChannelPicker.SelectedIndex < 0)
        {
            return;
        }

        SelectedChannel = ChannelPicker.SelectedIndex;
    }
}
