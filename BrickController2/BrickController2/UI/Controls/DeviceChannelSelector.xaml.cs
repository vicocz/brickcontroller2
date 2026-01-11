using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceChannelSelector : ContentView
    {
        public DeviceChannelSelector()
        {
            InitializeComponent();

            SBrickChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            SBrickChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            SBrickChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            SBrickChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
            BuWizzChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            BuWizzChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BuWizzChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            BuWizzChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
            BuWizz3Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
            BuWizz3Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BuWizz3Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
            BuWizz3Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
            BuWizz3Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
            BuWizz3Channel5.Command = new SafeCommand(() => SelectedChannel = 5);
            InfraredChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            InfraredChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            PoweredUpChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            PoweredUpChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            BoostChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
            BoostChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
            BoostChannelC.Command = new SafeCommand(() => SelectedChannel = 2);
            BoostChannelD.Command = new SafeCommand(() => SelectedChannel = 3);
            TechnicHubChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            TechnicHubChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            TechnicHubChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
            TechnicHubChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
            DuploTrainHubChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            CircuitCubesA.Command = new SafeCommand(() => SelectedChannel = 0);
            CircuitCubesB.Command = new SafeCommand(() => SelectedChannel = 1);
            CircuitCubesC.Command = new SafeCommand(() => SelectedChannel = 2);
            WedoChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            WedoChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            TechnicMoveChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
            TechnicMoveChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
            TechnicMoveChannelAB.Command = new SafeCommand(() => SelectedChannel = TechnicMoveDevice.CHANNEL_VM);
            TechnicMoveChannelC.Command = new SafeCommand(() => SelectedChannel = 2);
            TechnicMoveChannel1.Command = new SafeCommand(() => SelectedChannel = 3);
            TechnicMoveChannel2.Command = new SafeCommand(() => SelectedChannel = 4);
            TechnicMoveChannel3.Command = new SafeCommand(() => SelectedChannel = 5);
            TechnicMoveChannel4.Command = new SafeCommand(() => SelectedChannel = 6);
            TechnicMoveChannel5.Command = new SafeCommand(() => SelectedChannel = 7);
            TechnicMoveChannel6.Command = new SafeCommand(() => SelectedChannel = 8);
            PfxBrickChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
            PfxBrickChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
            PfxBrickChannel1.Command = new SafeCommand(() => SelectedChannel = 2);
            PfxBrickChannel2.Command = new SafeCommand(() => SelectedChannel = 3);
            PfxBrickChannel3.Command = new SafeCommand(() => SelectedChannel = 4);
            PfxBrickChannel4.Command = new SafeCommand(() => SelectedChannel = 5);
            PfxBrickChannel5.Command = new SafeCommand(() => SelectedChannel = 6);
            PfxBrickChannel6.Command = new SafeCommand(() => SelectedChannel = 7);
            PfxBrickChannel7.Command = new SafeCommand(() => SelectedChannel = 8);
            PfxBrickChannel8.Command = new SafeCommand(() => SelectedChannel = 9);
            MK3_8Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
            MK3_8Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
            MK3_8Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
            MK3_8Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
            MK3_8Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
            MK4Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
            MK4Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
            MK4Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
            MK4Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
            MK5Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
            MK5Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
            MK5Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
            MK5Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
            MK5Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
            MK6Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
            MK6Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
            MK6Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
            MK6Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
            MK6Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
            MK6Channel5.Command = new SafeCommand(() => SelectedChannel = 5);
            CaDARaceCarChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
            CaDARaceCarChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
            CaDARaceCarChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
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
                // enforce update
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
            var deviceType = device.DeviceType;
            SbrickSection.IsVisible = deviceType == DeviceType.SBrick;
            BuWizzSection.IsVisible = deviceType == DeviceType.BuWizz || deviceType == DeviceType.BuWizz2;
            BuWizz3Section.IsVisible = deviceType == DeviceType.BuWizz3;
            InfraredSection.IsVisible = deviceType == DeviceType.Infrared;
            PoweredUpSection.IsVisible = deviceType == DeviceType.PoweredUp;
            BoostSection.IsVisible = deviceType == DeviceType.Boost;
            TechnicHubSection.IsVisible = deviceType == DeviceType.TechnicHub;
            DuploTrainHubSection.IsVisible = deviceType == DeviceType.DuploTrainHub;
            CircuitCubes.IsVisible = deviceType == DeviceType.CircuitCubes;
            Wedo2Section.IsVisible = deviceType == DeviceType.WeDo2;
            // Technic Move enablement
            var isPlayVm = device is TechnicMoveDevice moveDevice && moveDevice.EnablePlayVmMode;
            TechnicMoveSection.IsVisible = deviceType == DeviceType.TechnicMove;
            TechnicMoveChannelA.IsVisible = !isPlayVm;
            TechnicMoveChannelB.IsVisible = !isPlayVm;
            TechnicMoveChannelAB.IsVisible = isPlayVm;
            PfxBrickSection.IsVisible = deviceType == DeviceType.PfxBrick;
            MK3_8Section.IsVisible = deviceType == DeviceType.MK3_8;
            MK4Section.IsVisible = deviceType == DeviceType.MK4;
            MK5Section.IsVisible = deviceType == DeviceType.MK5;
            MK6Section.IsVisible = deviceType == DeviceType.MK6;
            MK_DIYSection.IsVisible = deviceType == DeviceType.MK_DIY;
            CaDARaceCarSection.IsVisible = deviceType == DeviceType.CaDA_RaceCar;
        }

        private static void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelSelector dcs)
            {
                int selectedChannel = (int)newValue;
                dcs.OnSelectedChannelChanged(selectedChannel);
            }
        }

        private void OnSelectedChannelChanged(int selectedChannel)
        {
            SBrickChannel0.SelectedChannel = selectedChannel;
            SBrickChannel1.SelectedChannel = selectedChannel;
            SBrickChannel2.SelectedChannel = selectedChannel;
            SBrickChannel3.SelectedChannel = selectedChannel;
            BuWizzChannel0.SelectedChannel = selectedChannel;
            BuWizzChannel1.SelectedChannel = selectedChannel;
            BuWizzChannel2.SelectedChannel = selectedChannel;
            BuWizzChannel3.SelectedChannel = selectedChannel;
            BuWizz3Channel0.SelectedChannel = selectedChannel;
            BuWizz3Channel1.SelectedChannel = selectedChannel;
            BuWizz3Channel2.SelectedChannel = selectedChannel;
            BuWizz3Channel3.SelectedChannel = selectedChannel;
            BuWizz3Channel4.SelectedChannel = selectedChannel;
            BuWizz3Channel5.SelectedChannel = selectedChannel;
            InfraredChannel0.SelectedChannel = selectedChannel;
            InfraredChannel1.SelectedChannel = selectedChannel;
            PoweredUpChannel0.SelectedChannel = selectedChannel;
            PoweredUpChannel1.SelectedChannel = selectedChannel;
            BoostChannelA.SelectedChannel = selectedChannel;
            BoostChannelB.SelectedChannel = selectedChannel;
            BoostChannelC.SelectedChannel = selectedChannel;
            BoostChannelD.SelectedChannel = selectedChannel;
            TechnicHubChannel0.SelectedChannel = selectedChannel;
            TechnicHubChannel1.SelectedChannel = selectedChannel;
            TechnicHubChannel2.SelectedChannel = selectedChannel;
            TechnicHubChannel3.SelectedChannel = selectedChannel;
            DuploTrainHubChannel0.SelectedChannel = selectedChannel;
            CircuitCubesA.SelectedChannel = selectedChannel;
            CircuitCubesB.SelectedChannel = selectedChannel;
            CircuitCubesC.SelectedChannel = selectedChannel;
            WedoChannel0.SelectedChannel = selectedChannel;
            WedoChannel1.SelectedChannel = selectedChannel;
            TechnicMoveChannelA.SelectedChannel = selectedChannel;
            TechnicMoveChannelB.SelectedChannel = selectedChannel;
            TechnicMoveChannelAB.SelectedChannel = selectedChannel;
            TechnicMoveChannelC.SelectedChannel = selectedChannel;
            TechnicMoveChannel1.SelectedChannel = selectedChannel;
            TechnicMoveChannel2.SelectedChannel = selectedChannel;
            TechnicMoveChannel3.SelectedChannel = selectedChannel;
            TechnicMoveChannel4.SelectedChannel = selectedChannel;
            TechnicMoveChannel5.SelectedChannel = selectedChannel;
            TechnicMoveChannel6.SelectedChannel = selectedChannel;
            PfxBrickChannelA.SelectedChannel = selectedChannel;
            PfxBrickChannelB.SelectedChannel = selectedChannel;
            PfxBrickChannel1.SelectedChannel = selectedChannel;
            PfxBrickChannel2.SelectedChannel = selectedChannel;
            PfxBrickChannel3.SelectedChannel = selectedChannel;
            PfxBrickChannel4.SelectedChannel = selectedChannel;
            PfxBrickChannel5.SelectedChannel = selectedChannel;
            PfxBrickChannel6.SelectedChannel = selectedChannel;
            PfxBrickChannel7.SelectedChannel = selectedChannel;
            PfxBrickChannel8.SelectedChannel = selectedChannel;
            MK3_8Channel0.SelectedChannel = selectedChannel;
            MK3_8Channel1.SelectedChannel = selectedChannel;
            MK3_8Channel2.SelectedChannel = selectedChannel;
            MK3_8Channel3.SelectedChannel = selectedChannel;
            MK3_8Channel4.SelectedChannel = selectedChannel;
            MK4Channel0.SelectedChannel = selectedChannel;
            MK4Channel1.SelectedChannel = selectedChannel;
            MK4Channel2.SelectedChannel = selectedChannel;
            MK4Channel3.SelectedChannel = selectedChannel;
            MK5Channel0.SelectedChannel = selectedChannel;
            MK5Channel1.SelectedChannel = selectedChannel;
            MK5Channel2.SelectedChannel = selectedChannel;
            MK5Channel3.SelectedChannel = selectedChannel;
            MK5Channel4.SelectedChannel = selectedChannel;
            MK6Channel0.SelectedChannel = selectedChannel;
            MK6Channel1.SelectedChannel = selectedChannel;
            MK6Channel2.SelectedChannel = selectedChannel;
            MK6Channel3.SelectedChannel = selectedChannel;
            MK6Channel4.SelectedChannel = selectedChannel;
            MK6Channel5.SelectedChannel = selectedChannel;
            MK_DIYChannel0.SelectedChannel = selectedChannel;
            MK_DIYChannel1.SelectedChannel = selectedChannel;
            MK_DIYChannel2.SelectedChannel = selectedChannel;
            MK_DIYChannel3.SelectedChannel = selectedChannel;
            CaDARaceCarChannel0.SelectedChannel = selectedChannel;
            CaDARaceCarChannel1.SelectedChannel = selectedChannel;
            CaDARaceCarChannel2.SelectedChannel = selectedChannel;
        }
    }
}