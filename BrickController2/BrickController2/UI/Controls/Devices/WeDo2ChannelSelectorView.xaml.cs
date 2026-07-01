using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class WeDo2ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public WeDo2ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(WedoChannel0, WedoChannel1);
    }
}
