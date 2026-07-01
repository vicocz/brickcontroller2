using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK4ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK4ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK4Channel0, MK4Channel1, MK4Channel2, MK4Channel3);
    }
}
