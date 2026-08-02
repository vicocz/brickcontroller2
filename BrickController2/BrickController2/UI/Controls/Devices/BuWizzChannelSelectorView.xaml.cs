using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BuWizzChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.BuWizz;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public BuWizzChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(BuWizzChannel0, BuWizzChannel1, BuWizzChannel2, BuWizzChannel3);
    }
}
