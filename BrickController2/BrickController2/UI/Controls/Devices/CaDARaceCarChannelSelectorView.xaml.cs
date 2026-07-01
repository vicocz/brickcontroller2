using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CaDARaceCarChannelSelectorView : DeviceChannelSelectorViewBase
{
    public CaDARaceCarChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(CaDARaceCarChannel0, CaDARaceCarChannel1, CaDARaceCarChannel2);
    }
}
