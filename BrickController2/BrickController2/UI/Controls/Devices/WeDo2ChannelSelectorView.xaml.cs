using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class WeDo2ChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.WeDo2;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public WeDo2ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(WedoChannel0, WedoChannel1);
    }
}
