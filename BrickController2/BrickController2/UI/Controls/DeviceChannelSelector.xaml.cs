using System;
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
            var required = GetDeviceTypeKey(device);

            if (_activeView is null || _activeView.GetType() != required)
            {
                _activeView = CreateViewFor(device);
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

        private static System.Type GetDeviceTypeKey(Device device) => device.DeviceType switch
        {
            DeviceType.SBrick         => typeof(SBrickChannelSelectorView),
            DeviceType.SBrickLight    => typeof(SBrickLightChannelSelectorView),
            DeviceType.BuWizz
            or DeviceType.BuWizz2    => typeof(BuWizzChannelSelectorView),
            DeviceType.BuWizz3       => typeof(BuWizz3ChannelSelectorView),
            DeviceType.Infrared      => typeof(PowerFunctionsChannelSelectorView),
            DeviceType.PoweredUp     => typeof(PoweredUpChannelSelectorView),
            DeviceType.Boost         => typeof(BoostChannelSelectorView),
            DeviceType.TechnicHub    => typeof(TechnicHubChannelSelectorView),
            DeviceType.DuploTrainHub => typeof(DuploTrainHubChannelSelectorView),
            DeviceType.CircuitCubes  => typeof(CircuitCubesChannelSelectorView),
            DeviceType.WeDo2         => typeof(WeDo2ChannelSelectorView),
            DeviceType.TechnicMove   => typeof(TechnicMoveChannelSelectorView),
            DeviceType.PfxBrick      => typeof(PfxBrickChannelSelectorView),
            DeviceType.MK3_8         => typeof(MK3_8ChannelSelectorView),
            DeviceType.MK4           => typeof(MK4ChannelSelectorView),
            DeviceType.MK5           => typeof(MK5ChannelSelectorView),
            DeviceType.MK6           => typeof(MK6ChannelSelectorView),
            DeviceType.MK_DIY        => typeof(MK_DIYChannelSelectorView),
            DeviceType.CaDA_RaceCar  => typeof(CaDARaceCarChannelSelectorView),
            DeviceType.JieStarSCM4   => typeof(JieStarSCM4ChannelSelectorView),
            DeviceType.JieStarSCM8   => typeof(JieStarSCM8ChannelSelectorView),
            _                        => typeof(DeviceChannelSelectorViewBase)
        };

        private static DeviceChannelSelectorViewBase CreateViewFor(Device device) => device.DeviceType switch
        {
            DeviceType.SBrick         => new SBrickChannelSelectorView(),
            DeviceType.SBrickLight    => new SBrickLightChannelSelectorView(),
            DeviceType.BuWizz
            or DeviceType.BuWizz2    => new BuWizzChannelSelectorView(),
            DeviceType.BuWizz3       => new BuWizz3ChannelSelectorView(),
            DeviceType.Infrared      => new PowerFunctionsChannelSelectorView(),
            DeviceType.PoweredUp     => new PoweredUpChannelSelectorView(),
            DeviceType.Boost         => new BoostChannelSelectorView(),
            DeviceType.TechnicHub    => new TechnicHubChannelSelectorView(),
            DeviceType.DuploTrainHub => new DuploTrainHubChannelSelectorView(),
            DeviceType.CircuitCubes  => new CircuitCubesChannelSelectorView(),
            DeviceType.WeDo2         => new WeDo2ChannelSelectorView(),
            DeviceType.TechnicMove   => new TechnicMoveChannelSelectorView(),
            DeviceType.PfxBrick      => new PfxBrickChannelSelectorView(),
            DeviceType.MK3_8         => new MK3_8ChannelSelectorView(),
            DeviceType.MK4           => new MK4ChannelSelectorView(),
            DeviceType.MK5           => new MK5ChannelSelectorView(),
            DeviceType.MK6           => new MK6ChannelSelectorView(),
            DeviceType.MK_DIY        => new MK_DIYChannelSelectorView(),
            DeviceType.CaDA_RaceCar  => new CaDARaceCarChannelSelectorView(),
            DeviceType.JieStarSCM4   => new JieStarSCM4ChannelSelectorView(),
            DeviceType.JieStarSCM8   => new JieStarSCM8ChannelSelectorView(),
            _                        => throw new NotSupportedException($"No channel selector for device type {device.DeviceType}")
        };
    }
}
