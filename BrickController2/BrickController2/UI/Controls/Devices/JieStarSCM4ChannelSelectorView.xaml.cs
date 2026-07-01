using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class JieStarSCM4ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public JieStarSCM4ChannelSelectorView()
    {
        InitializeComponent();

        JieStarSCM4Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        JieStarSCM4Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        JieStarSCM4Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        JieStarSCM4Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        JieStarSCM4Channel0.SelectedChannel = channel;
        JieStarSCM4Channel1.SelectedChannel = channel;
        JieStarSCM4Channel2.SelectedChannel = channel;
        JieStarSCM4Channel3.SelectedChannel = channel;
    }
}
