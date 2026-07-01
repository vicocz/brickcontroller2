using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SBrickChannelSelectorView : DeviceChannelSelectorViewBase
{
    public SBrickChannelSelectorView()
    {
        InitializeComponent();

        SBrickChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        SBrickChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
        SBrickChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
        SBrickChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        SBrickChannel0.SelectedChannel = channel;
        SBrickChannel1.SelectedChannel = channel;
        SBrickChannel2.SelectedChannel = channel;
        SBrickChannel3.SelectedChannel = channel;
    }
}
