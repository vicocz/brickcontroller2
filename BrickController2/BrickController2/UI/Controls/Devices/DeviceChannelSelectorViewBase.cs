using System.Collections.Generic;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Controls;
using Microsoft.Maui.Controls;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls.Devices;

public abstract class DeviceChannelSelectorViewBase : ContentView
{
    /// <summary>
    /// The <see cref="DeviceType"/> whose icon/label should be stamped on every registered button.
    /// Override via the child's static <c>DeviceType</c> property by returning it here.
    /// </summary>
    protected abstract DeviceType SelectorDeviceType { get; }
    private readonly List<ChannelSelectorRadioButton> _channelButtons = new();

    public static readonly BindableProperty DeviceProperty = BindableProperty.Create(
        nameof(Device),
        typeof(Device),
        typeof(DeviceChannelSelectorViewBase),
        default(Device),
        BindingMode.OneWay,
        propertyChanged: OnDeviceChangedStatic,
        coerceValue: OnCoerceDevice);

    public static readonly BindableProperty SelectedChannelProperty = BindableProperty.Create(
        nameof(SelectedChannel),
        typeof(int),
        typeof(DeviceChannelSelectorViewBase),
        0,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedChannelChangedStatic);

    public Device Device
    {
        get => (Device)GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    public int SelectedChannel
    {
        get => (int)GetValue(SelectedChannelProperty);
        set => SetValue(SelectedChannelProperty, value);
    }

    /// <summary>
    /// Registers channel buttons: auto-wires each button's Command to set
    /// SelectedChannel = button.Channel, and propagates SelectedChannel
    /// changes back to all registered buttons via OnSelectedChannelChanged.
    /// </summary>
    protected void RegisterChannelButtons(params ChannelSelectorRadioButton[] buttons)
    {
        var deviceType = SelectorDeviceType;
        foreach (var button in buttons)
        {
            button.DeviceType = deviceType;
            var captured = button;
            captured.Command = new SafeCommand(() => SelectedChannel = captured.Channel);
            _channelButtons.Add(captured);
        }
    }

    private static object OnCoerceDevice(BindableObject bindable, object value)
    {
        if (bindable is DeviceChannelSelectorViewBase view && value is Device device)
            view.OnDeviceChanged(device);
        return value;
    }

    private static void OnDeviceChangedStatic(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DeviceChannelSelectorViewBase view && newValue is Device device)
            view.OnDeviceChanged(device);
    }

    private static void OnSelectedChannelChangedStatic(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DeviceChannelSelectorViewBase view && newValue is int channel)
            view.OnSelectedChannelChanged(channel);
    }

    protected virtual void OnDeviceChanged(Device device) { }

    protected virtual void OnSelectedChannelChanged(int channel)
    {
        foreach (var button in _channelButtons)
            button.SelectedChannel = channel;
    }
}
