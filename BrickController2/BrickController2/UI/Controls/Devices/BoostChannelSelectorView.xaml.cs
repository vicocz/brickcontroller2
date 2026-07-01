using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BoostChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.Boost;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public BoostChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(BoostChannelA, BoostChannelB, BoostChannelC, BoostChannelD);
    }
}
