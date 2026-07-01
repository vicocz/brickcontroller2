using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SBrickChannelSelectorView : DeviceChannelSelectorViewBase
{
    public SBrickChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(SBrickChannel0, SBrickChannel1, SBrickChannel2, SBrickChannel3);
    }
}
