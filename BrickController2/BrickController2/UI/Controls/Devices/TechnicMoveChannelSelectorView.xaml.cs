using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TechnicMoveChannelSelectorView : DeviceChannelSelectorViewBase
{
    public TechnicMoveChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(TechnicMoveChannelA, TechnicMoveChannelB, TechnicMoveChannelAB, TechnicMoveChannelC, TechnicMoveChannel1, TechnicMoveChannel2, TechnicMoveChannel3, TechnicMoveChannel4, TechnicMoveChannel5, TechnicMoveChannel6);
    }

    protected override void OnDeviceChanged(Device device)
    {
        var isPlayVm = device is TechnicMoveDevice moveDevice && moveDevice.EnablePlayVmMode;
        TechnicMoveChannelA.IsVisible = !isPlayVm;
        TechnicMoveChannelB.IsVisible = !isPlayVm;
        TechnicMoveChannelAB.IsVisible = isPlayVm;
    }
}
