using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TechnicHubChannelSelectorView : DeviceChannelSelectorViewBase
{
    public TechnicHubChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(TechnicHubChannel0, TechnicHubChannel1, TechnicHubChannel2, TechnicHubChannel3);
    }
}
