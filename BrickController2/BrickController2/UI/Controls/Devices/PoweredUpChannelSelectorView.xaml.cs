using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PoweredUpChannelSelectorView : DeviceChannelSelectorViewBase
{
    public PoweredUpChannelSelectorView()
    {
        InitializeComponent();

        PoweredUpChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        PoweredUpChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        PoweredUpChannel0.SelectedChannel = channel;
        PoweredUpChannel1.SelectedChannel = channel;
    }
}
