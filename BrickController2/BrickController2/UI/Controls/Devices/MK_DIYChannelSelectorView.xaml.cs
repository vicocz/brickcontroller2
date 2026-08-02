using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK_DIYChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.MK_DIY;
    // Buttons intentionally show MK4 icons — preserved from original XAML
    protected override DeviceType SelectorDeviceType => DeviceType.MK4;

    public MK_DIYChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK_DIYChannel0, MK_DIYChannel1, MK_DIYChannel2, MK_DIYChannel3);
    }
}
