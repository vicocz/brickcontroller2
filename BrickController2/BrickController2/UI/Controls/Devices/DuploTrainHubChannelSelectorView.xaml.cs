using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DuploTrainHubChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.DuploTrainHub;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public DuploTrainHubChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(DuploTrainHubChannel0);
    }
}
