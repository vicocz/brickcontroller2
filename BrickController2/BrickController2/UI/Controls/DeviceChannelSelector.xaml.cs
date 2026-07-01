using System;
using System.Collections.Generic;
using BrickController2.DeviceManagement;
using BrickController2.UI.Controls.Devices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceChannelSelector : ContentView
    {
        private readonly record struct ViewEntry(Type ViewType, Func<DeviceChannelSelectorViewBase> Factory);

        /// <summary>
        /// Registry built once at class load from each view's static <see cref="IDeviceChannelSelectorView.DeviceType"/>.
        /// Adding a new device: implement <see cref="IDeviceChannelSelectorView"/> on the new view and add one entry here.
        /// </summary>
        private static readonly Dictionary<DeviceType, ViewEntry> _registry = BuildRegistry();

        private static Dictionary<DeviceType, ViewEntry> BuildRegistry()
        {
            static ViewEntry Entry<TView>(Func<TView> factory)
                where TView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
                => new(typeof(TView), factory);

            var registry = new Dictionary<DeviceType, ViewEntry>
            {
                [SBrickChannelSelectorView.DeviceType]         = Entry(() => new SBrickChannelSelectorView()),
                [SBrickLightChannelSelectorView.DeviceType]    = Entry(() => new SBrickLightChannelSelectorView()),
                [BuWizzChannelSelectorView.DeviceType]         = Entry(() => new BuWizzChannelSelectorView()),
                [BuWizz3ChannelSelectorView.DeviceType]        = Entry(() => new BuWizz3ChannelSelectorView()),
                [PowerFunctionsChannelSelectorView.DeviceType] = Entry(() => new PowerFunctionsChannelSelectorView()),
                [PoweredUpChannelSelectorView.DeviceType]      = Entry(() => new PoweredUpChannelSelectorView()),
                [BoostChannelSelectorView.DeviceType]          = Entry(() => new BoostChannelSelectorView()),
                [TechnicHubChannelSelectorView.DeviceType]     = Entry(() => new TechnicHubChannelSelectorView()),
                [DuploTrainHubChannelSelectorView.DeviceType]  = Entry(() => new DuploTrainHubChannelSelectorView()),
                [CircuitCubesChannelSelectorView.DeviceType]   = Entry(() => new CircuitCubesChannelSelectorView()),
                [WeDo2ChannelSelectorView.DeviceType]          = Entry(() => new WeDo2ChannelSelectorView()),
                [TechnicMoveChannelSelectorView.DeviceType]    = Entry(() => new TechnicMoveChannelSelectorView()),
                [PfxBrickChannelSelectorView.DeviceType]       = Entry(() => new PfxBrickChannelSelectorView()),
                [MK3_8ChannelSelectorView.DeviceType]          = Entry(() => new MK3_8ChannelSelectorView()),
                [MK4ChannelSelectorView.DeviceType]            = Entry(() => new MK4ChannelSelectorView()),
                [MK5ChannelSelectorView.DeviceType]            = Entry(() => new MK5ChannelSelectorView()),
                [MK6ChannelSelectorView.DeviceType]            = Entry(() => new MK6ChannelSelectorView()),
                [MK_DIYChannelSelectorView.DeviceType]         = Entry(() => new MK_DIYChannelSelectorView()),
                [CaDARaceCarChannelSelectorView.DeviceType]    = Entry(() => new CaDARaceCarChannelSelectorView()),
                [JieStarSCM4ChannelSelectorView.DeviceType]   = Entry(() => new JieStarSCM4ChannelSelectorView()),
                [JieStarSCM8ChannelSelectorView.DeviceType]   = Entry(() => new JieStarSCM8ChannelSelectorView()),
            };

            // BuWizz2 shares the same view as BuWizz
            registry[DeviceType.BuWizz2] = registry[BuWizzChannelSelectorView.DeviceType];

            return registry;
        }

        private DeviceChannelSelectorViewBase? _activeView;

        public DeviceChannelSelector()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty DeviceProperty = BindableProperty.Create(nameof(Device), typeof(Device), typeof(DeviceChannelSelector), default(Device), BindingMode.OneWay, null, OnDeviceChanged, coerceValue: OnCoerceDevice);
        public static readonly BindableProperty SelectedChannelProperty = BindableProperty.Create(nameof(SelectedChannel), typeof(int), typeof(DeviceChannelSelector), 0, BindingMode.TwoWay, null, OnSelectedChannelChanged);

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
            if (!_registry.TryGetValue(device.DeviceType, out var entry))
                throw new NotSupportedException($"No channel selector for device type {device.DeviceType}");

            if (_activeView is null || _activeView.GetType() != entry.ViewType)
            {
                _activeView = entry.Factory();
                _activeView.BindingContext = BindingContext;
                _activeView.SelectedChannel = SelectedChannel;

                // forward SelectedChannel changes from the child back up to the parent bindable property
                _activeView.SetBinding(DeviceChannelSelectorViewBase.SelectedChannelProperty,
                    new Binding(nameof(SelectedChannel), source: this, mode: BindingMode.TwoWay));

                DeviceContent.Content = _activeView;
            }

            _activeView.Device = device;
        }

        private static void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs && dcs._activeView is not null)
            {
                dcs._activeView.SelectedChannel = (int)newValue;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (_activeView is not null)
                _activeView.BindingContext = BindingContext;
        }
    }
}
