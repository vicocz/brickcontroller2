using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK5ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK5ChannelSelectorView()
    {
        InitializeComponent();

        MK5Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        MK5Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        MK5Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        MK5Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
        MK5Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        MK5Channel0.SelectedChannel = channel;
        MK5Channel1.SelectedChannel = channel;
        MK5Channel2.SelectedChannel = channel;
        MK5Channel3.SelectedChannel = channel;
        MK5Channel4.SelectedChannel = channel;
    }
}
