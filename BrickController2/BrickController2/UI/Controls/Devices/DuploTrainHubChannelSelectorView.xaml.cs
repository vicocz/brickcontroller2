using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DuploTrainHubChannelSelectorView : DeviceChannelSelectorViewBase
{
    public DuploTrainHubChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(DuploTrainHubChannel0);
    }
}
