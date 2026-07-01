using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK5ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public MK5ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK5Channel0, MK5Channel1, MK5Channel2, MK5Channel3, MK5Channel4);
    }
}
