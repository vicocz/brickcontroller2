using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BoostChannelSelectorView : DeviceChannelSelectorViewBase
{
    public BoostChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(BoostChannelA, BoostChannelB, BoostChannelC, BoostChannelD);
    }
}
