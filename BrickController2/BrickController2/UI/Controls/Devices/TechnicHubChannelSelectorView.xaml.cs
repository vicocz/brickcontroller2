using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TechnicHubChannelSelectorView : DeviceChannelSelectorViewBase
{
    public TechnicHubChannelSelectorView()
    {
        InitializeComponent();

        TechnicHubChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        TechnicHubChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
        TechnicHubChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
        TechnicHubChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        TechnicHubChannel0.SelectedChannel = channel;
        TechnicHubChannel1.SelectedChannel = channel;
        TechnicHubChannel2.SelectedChannel = channel;
        TechnicHubChannel3.SelectedChannel = channel;
    }
}
