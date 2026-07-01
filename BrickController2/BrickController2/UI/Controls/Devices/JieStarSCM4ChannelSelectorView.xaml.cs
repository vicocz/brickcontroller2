using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class JieStarSCM4ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public JieStarSCM4ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(JieStarSCM4Channel0, JieStarSCM4Channel1, JieStarSCM4Channel2, JieStarSCM4Channel3);
    }
}
