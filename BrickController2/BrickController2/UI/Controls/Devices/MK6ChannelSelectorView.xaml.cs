using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK6ChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.MK6;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public MK6ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK6Channel0, MK6Channel1, MK6Channel2, MK6Channel3, MK6Channel4, MK6Channel5);
    }
}
