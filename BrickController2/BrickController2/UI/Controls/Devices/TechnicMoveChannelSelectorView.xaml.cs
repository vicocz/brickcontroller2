using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TechnicMoveChannelSelectorView : DeviceChannelSelectorViewBase
{
    public TechnicMoveChannelSelectorView()
    {
        InitializeComponent();

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
    }

    protected override void OnDeviceChanged(Device device)
    {
        var isPlayVm = device is TechnicMoveDevice moveDevice && moveDevice.EnablePlayVmMode;
        TechnicMoveChannelA.IsVisible = !isPlayVm;
        TechnicMoveChannelB.IsVisible = !isPlayVm;
        TechnicMoveChannelAB.IsVisible = isPlayVm;
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        TechnicMoveChannelA.SelectedChannel = channel;
        TechnicMoveChannelB.SelectedChannel = channel;
        TechnicMoveChannelAB.SelectedChannel = channel;
        TechnicMoveChannelC.SelectedChannel = channel;
        TechnicMoveChannel1.SelectedChannel = channel;
        TechnicMoveChannel2.SelectedChannel = channel;
        TechnicMoveChannel3.SelectedChannel = channel;
        TechnicMoveChannel4.SelectedChannel = channel;
        TechnicMoveChannel5.SelectedChannel = channel;
        TechnicMoveChannel6.SelectedChannel = channel;
    }
}
