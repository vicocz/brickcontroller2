using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class JieStarSCM8ChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.JieStarSCM8;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public JieStarSCM8ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(JieStarSCM8Channel0, JieStarSCM8Channel1, JieStarSCM8Channel2, JieStarSCM8Channel3, JieStarSCM8Channel4, JieStarSCM8Channel5, JieStarSCM8Channel6, JieStarSCM8Channel7);
    }
}
