using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PowerFunctionsChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.Infrared;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public PowerFunctionsChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(PowerFunctionsChannel0, PowerFunctionsChannel1);
    }
}
