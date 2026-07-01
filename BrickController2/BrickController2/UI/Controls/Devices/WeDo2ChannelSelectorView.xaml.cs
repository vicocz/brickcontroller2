using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class WeDo2ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public WeDo2ChannelSelectorView()
    {
        InitializeComponent();

        WedoChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        WedoChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        WedoChannel0.SelectedChannel = channel;
        WedoChannel1.SelectedChannel = channel;
    }
}
