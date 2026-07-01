using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PfxBrickChannelSelectorView : DeviceChannelSelectorViewBase
{
    public PfxBrickChannelSelectorView()
    {
        InitializeComponent();

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
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        PfxBrickChannelA.SelectedChannel = channel;
        PfxBrickChannelB.SelectedChannel = channel;
        PfxBrickChannel1.SelectedChannel = channel;
        PfxBrickChannel2.SelectedChannel = channel;
        PfxBrickChannel3.SelectedChannel = channel;
        PfxBrickChannel4.SelectedChannel = channel;
        PfxBrickChannel5.SelectedChannel = channel;
        PfxBrickChannel6.SelectedChannel = channel;
        PfxBrickChannel7.SelectedChannel = channel;
        PfxBrickChannel8.SelectedChannel = channel;
    }
}
