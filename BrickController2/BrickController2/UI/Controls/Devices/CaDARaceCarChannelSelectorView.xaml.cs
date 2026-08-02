using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CaDARaceCarChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.CaDA_RaceCar;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public CaDARaceCarChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(CaDARaceCarChannel0, CaDARaceCarChannel1, CaDARaceCarChannel2);
    }
}
