using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK3_8ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK3_8ChannelSelectorView()
    {
        InitializeComponent();

        MK3_8Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        MK3_8Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        MK3_8Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        MK3_8Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
        MK3_8Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        MK3_8Channel0.SelectedChannel = channel;
        MK3_8Channel1.SelectedChannel = channel;
        MK3_8Channel2.SelectedChannel = channel;
        MK3_8Channel3.SelectedChannel = channel;
        MK3_8Channel4.SelectedChannel = channel;
    }
}
