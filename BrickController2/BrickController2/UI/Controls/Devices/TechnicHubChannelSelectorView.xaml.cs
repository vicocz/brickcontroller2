using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TechnicHubChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.TechnicHub;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public TechnicHubChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(TechnicHubChannel0, TechnicHubChannel1, TechnicHubChannel2, TechnicHubChannel3);
    }
}
