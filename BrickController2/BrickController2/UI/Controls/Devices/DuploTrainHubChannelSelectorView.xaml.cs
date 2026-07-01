using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DuploTrainHubChannelSelectorView : DeviceChannelSelectorViewBase
{
    public DuploTrainHubChannelSelectorView()
    {
        InitializeComponent();

        DuploTrainHubChannel0.Command = new SafeCommand(() => SelectedChannel = 0);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        DuploTrainHubChannel0.SelectedChannel = channel;
    }
}
