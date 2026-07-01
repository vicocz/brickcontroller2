using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PoweredUpChannelSelectorView : DeviceChannelSelectorViewBase
{
    public PoweredUpChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(PoweredUpChannel0, PoweredUpChannel1);
    }
}
