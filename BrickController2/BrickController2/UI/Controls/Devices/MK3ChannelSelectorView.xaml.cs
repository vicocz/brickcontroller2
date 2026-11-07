using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK3ChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.MK3;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public MK3ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK3Channel0, MK3Channel1, MK3Channel2, MK3Channel3);
    }
}
