using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK_DIYChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK_DIYChannelSelectorView()
    {
        InitializeComponent();

        MK_DIYChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        MK_DIYChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
        MK_DIYChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
        MK_DIYChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        MK_DIYChannel0.SelectedChannel = channel;
        MK_DIYChannel1.SelectedChannel = channel;
        MK_DIYChannel2.SelectedChannel = channel;
        MK_DIYChannel3.SelectedChannel = channel;
    }
}
