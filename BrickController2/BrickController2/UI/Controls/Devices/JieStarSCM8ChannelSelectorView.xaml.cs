using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class JieStarSCM8ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public JieStarSCM8ChannelSelectorView()
    {
        InitializeComponent();

        JieStarSCM8Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        JieStarSCM8Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        JieStarSCM8Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        JieStarSCM8Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
        JieStarSCM8Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
        JieStarSCM8Channel5.Command = new SafeCommand(() => SelectedChannel = 5);
        JieStarSCM8Channel6.Command = new SafeCommand(() => SelectedChannel = 6);
        JieStarSCM8Channel7.Command = new SafeCommand(() => SelectedChannel = 7);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        JieStarSCM8Channel0.SelectedChannel = channel;
        JieStarSCM8Channel1.SelectedChannel = channel;
        JieStarSCM8Channel2.SelectedChannel = channel;
        JieStarSCM8Channel3.SelectedChannel = channel;
        JieStarSCM8Channel4.SelectedChannel = channel;
        JieStarSCM8Channel5.SelectedChannel = channel;
        JieStarSCM8Channel6.SelectedChannel = channel;
        JieStarSCM8Channel7.SelectedChannel = channel;
    }
}
