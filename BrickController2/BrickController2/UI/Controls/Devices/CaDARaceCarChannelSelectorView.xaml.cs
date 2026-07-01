using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CaDARaceCarChannelSelectorView : DeviceChannelSelectorViewBase
{
    public CaDARaceCarChannelSelectorView()
    {
        InitializeComponent();

        CaDARaceCarChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        CaDARaceCarChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
        CaDARaceCarChannel2.Command = new SafeCommand(() => SelectedChannel = 2);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        CaDARaceCarChannel0.SelectedChannel = channel;
        CaDARaceCarChannel1.SelectedChannel = channel;
        CaDARaceCarChannel2.SelectedChannel = channel;
    }
}
