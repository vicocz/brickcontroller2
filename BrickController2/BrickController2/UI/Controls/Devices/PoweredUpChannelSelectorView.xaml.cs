using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PoweredUpChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.PoweredUp;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public PoweredUpChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(PoweredUpChannel0, PoweredUpChannel1);
    }
}
