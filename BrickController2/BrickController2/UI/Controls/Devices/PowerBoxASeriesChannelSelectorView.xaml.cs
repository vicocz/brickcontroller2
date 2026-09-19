using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PowerBoxASeriesChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.PowerBoxASeries;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public PowerBoxASeriesChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(PowerBoxASeriesChannel0, PowerBoxASeriesChannel1, PowerBoxASeriesChannel2, PowerBoxASeriesChannel3);
    }
}
