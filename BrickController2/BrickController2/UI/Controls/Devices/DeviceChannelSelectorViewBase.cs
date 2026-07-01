using Microsoft.Maui.Controls;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls.Devices;

public abstract class DeviceChannelSelectorViewBase : ContentView
{
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

    protected abstract void OnSelectedChannelChanged(int channel);
}
