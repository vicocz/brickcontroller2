using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK4ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK4ChannelSelectorView()
    {
        InitializeComponent();

        MK4Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        MK4Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        MK4Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        MK4Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        MK4Channel0.SelectedChannel = channel;
        MK4Channel1.SelectedChannel = channel;
        MK4Channel2.SelectedChannel = channel;
        MK4Channel3.SelectedChannel = channel;
    }
}
