using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BuWizzChannelSelectorView : DeviceChannelSelectorViewBase
{
    public BuWizzChannelSelectorView()
    {
        InitializeComponent();

        BuWizzChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        BuWizzChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
        BuWizzChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
        BuWizzChannel3.Command = new SafeCommand(() => SelectedChannel = 3);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        BuWizzChannel0.SelectedChannel = channel;
        BuWizzChannel1.SelectedChannel = channel;
        BuWizzChannel2.SelectedChannel = channel;
        BuWizzChannel3.SelectedChannel = channel;
    }
}
