using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BoostChannelSelectorView : DeviceChannelSelectorViewBase
{
    public BoostChannelSelectorView()
    {
        InitializeComponent();

        BoostChannelA.Command = new SafeCommand(() => SelectedChannel = 0);
        BoostChannelB.Command = new SafeCommand(() => SelectedChannel = 1);
        BoostChannelC.Command = new SafeCommand(() => SelectedChannel = 2);
        BoostChannelD.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        BoostChannelA.SelectedChannel = channel;
        BoostChannelB.SelectedChannel = channel;
        BoostChannelC.SelectedChannel = channel;
        BoostChannelD.SelectedChannel = channel;
    }
}
