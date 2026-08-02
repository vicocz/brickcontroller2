using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.UI.Controls.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceChannelSelector : ContentView
    {
        private static IComponentContext? _componentContext;

        /// <summary>
        /// Lazily resolved Autofac context, used to resolve <see cref="DeviceChannelSelectorViewBase"/>
        /// instances keyed by <see cref="DeviceType"/> (see UiModule registration).
        /// </summary>
        private static IComponentContext ComponentContext =>
            _componentContext ??= IPlatformApplication.Current!.Services.GetRequiredService<IComponentContext>();

        private DeviceChannelSelectorViewBase? _activeView;
        private DeviceType? _activeDeviceType;

        public DeviceChannelSelector()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty DeviceProperty = BindableProperty.Create(nameof(Device), typeof(Device), typeof(DeviceChannelSelector), default(Device), BindingMode.OneWay, null, OnDeviceChanged, coerceValue: OnCoerceDevice);
        public static readonly BindableProperty SelectedChannelProperty = BindableProperty.Create(nameof(SelectedChannel), typeof(int), typeof(DeviceChannelSelector), 0, BindingMode.TwoWay);

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
            if (bindable is DeviceChannelSelector dcs && value is Device device)
            {
                // enforce update even when the same Device reference is re-set (e.g. TechnicMove PlayVM mode change)
                dcs.OnDeviceChanged(device);
            }
            return value;
        }

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs && newValue is Device device)
            {
                dcs.OnDeviceChanged(device);
            }
        }

        private void OnDeviceChanged(Device device)
        {
            if (_activeView is null || _activeDeviceType != device.DeviceType)
            {
                if (!ComponentContext.TryResolveKeyed<DeviceChannelSelectorViewBase>(device.DeviceType, out var resolved))
                {
                    // use generic component as fallback
                    resolved = new GenericChannelSelectorView();
                }

                if (_activeView is not null)
                {
                    // detach previous view to avoid it (and its binding to 'this') being kept alive
                    _activeView.RemoveBinding(DeviceChannelSelectorViewBase.SelectedChannelProperty);
                    _activeView.BindingContext = null;
                }

                _activeView = resolved;
                _activeDeviceType = device.DeviceType;
                _activeView.BindingContext = BindingContext;
                _activeView.SelectedChannel = SelectedChannel;

                // forward SelectedChannel changes from the child back up to the parent bindable property
                _activeView.SetBinding(DeviceChannelSelectorViewBase.SelectedChannelProperty,
                    new Binding(nameof(SelectedChannel), source: this, mode: BindingMode.TwoWay));

                DeviceContent.Content = _activeView;
            }

            _activeView.Device = device;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            _activeView?.BindingContext = BindingContext;
        }
    }
}
