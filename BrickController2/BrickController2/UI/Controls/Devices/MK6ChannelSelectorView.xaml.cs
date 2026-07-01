using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK6ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK6ChannelSelectorView()
    {
        InitializeComponent();

        MK6Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        MK6Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        MK6Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        MK6Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
        MK6Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
        MK6Channel5.Command = new SafeCommand(() => SelectedChannel = 5);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        MK6Channel0.SelectedChannel = channel;
        MK6Channel1.SelectedChannel = channel;
        MK6Channel2.SelectedChannel = channel;
        MK6Channel3.SelectedChannel = channel;
        MK6Channel4.SelectedChannel = channel;
        MK6Channel5.SelectedChannel = channel;
    }
}
