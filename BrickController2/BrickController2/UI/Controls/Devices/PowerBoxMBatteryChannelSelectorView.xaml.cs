using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PowerBoxMBatteryChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.PowerBoxMBattery;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public PowerBoxMBatteryChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(PowerBoxMBatteryChannel0);
    }
}
