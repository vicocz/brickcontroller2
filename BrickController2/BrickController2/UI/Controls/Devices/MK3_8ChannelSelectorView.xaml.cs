using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MK3_8ChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.MK3_8;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public MK3_8ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(MK3_8Channel0, MK3_8Channel1, MK3_8Channel2, MK3_8Channel3, MK3_8Channel4);
    }
}
