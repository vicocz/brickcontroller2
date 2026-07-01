using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK_DIYChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK_DIYChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK_DIYChannel0, MK_DIYChannel1, MK_DIYChannel2, MK_DIYChannel3);
    }
}
