using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PowerFunctionsChannelSelectorView : DeviceChannelSelectorViewBase
{
    public PowerFunctionsChannelSelectorView()
    {
        InitializeComponent();

        PowerFunctionsChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
        PowerFunctionsChannel1.Command = new SafeCommand(() => SelectedChannel = 1);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        PowerFunctionsChannel0.SelectedChannel = channel;
        PowerFunctionsChannel1.SelectedChannel = channel;
    }
}
