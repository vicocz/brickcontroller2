using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PfxBrickChannelSelectorView : DeviceChannelSelectorViewBase
{
    public PfxBrickChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(PfxBrickChannelA, PfxBrickChannelB, PfxBrickChannel1, PfxBrickChannel2, PfxBrickChannel3, PfxBrickChannel4, PfxBrickChannel5, PfxBrickChannel6, PfxBrickChannel7, PfxBrickChannel8);
    }
}
