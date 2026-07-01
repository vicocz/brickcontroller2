using BrickController2.DeviceManagement.Vengit;
using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SBrickLightChannelSelectorView : DeviceChannelSelectorViewBase
{
    public SBrickLightChannelSelectorView()
    {
        InitializeComponent();

        SBrickLightChannelA.Command = new SafeCommand(() => UpdateSBrickPort(0));
        SBrickLightChannelB.Command = new SafeCommand(() => UpdateSBrickPort(1));
        SBrickLightChannelC.Command = new SafeCommand(() => UpdateSBrickPort(2));
        SBrickLightChannelD.Command = new SafeCommand(() => UpdateSBrickPort(3));
        SBrickLightChannelE.Command = new SafeCommand(() => UpdateSBrickPort(4));
        SBrickLightChannelF.Command = new SafeCommand(() => UpdateSBrickPort(5));
        SBrickLightChannelG.Command = new SafeCommand(() => UpdateSBrickPort(6));
        SBrickLightChannelH.Command = new SafeCommand(() => UpdateSBrickPort(7));
        SBrickLightSubchannel1.Command = new SafeCommand(() => UpdateSBrickSubchannel(SBrickProtocol.LIGHT_PORTS_COUNT * 1));
        SBrickLightSubchannel2.Command = new SafeCommand(() => UpdateSBrickSubchannel(SBrickProtocol.LIGHT_PORTS_COUNT * 2));
        SBrickLightSubchannel3.Command = new SafeCommand(() => UpdateSBrickSubchannel(SBrickProtocol.LIGHT_PORTS_COUNT * 3));
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        var sBrickLightChannel = channel % SBrickProtocol.LIGHT_PORTS_COUNT;
        var sBrickLightSubchannel = channel < SBrickProtocol.LIGHT_PORTS_COUNT
            ? 0
            : channel / SBrickProtocol.LIGHT_PORTS_COUNT - 1;

        SBrickLightChannelA.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelB.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelC.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelD.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelE.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelF.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelG.SelectedChannel = sBrickLightChannel;
        SBrickLightChannelH.SelectedChannel = sBrickLightChannel;
        SBrickLightSubchannel1.SelectedChannel = sBrickLightSubchannel;
        SBrickLightSubchannel2.SelectedChannel = sBrickLightSubchannel;
        SBrickLightSubchannel3.SelectedChannel = sBrickLightSubchannel;
    }

    private void UpdateSBrickPort(int channel)
    {
        SelectedChannel = channel + (SelectedChannel / SBrickProtocol.LIGHT_PORTS_COUNT) * SBrickProtocol.LIGHT_PORTS_COUNT;
    }

    private void UpdateSBrickSubchannel(int subchannel)
    {
        SelectedChannel = subchannel + SelectedChannel % SBrickProtocol.LIGHT_PORTS_COUNT;
    }
}
