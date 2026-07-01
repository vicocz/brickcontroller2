using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CircuitCubesChannelSelectorView : DeviceChannelSelectorViewBase
{
    public CircuitCubesChannelSelectorView()
    {
        InitializeComponent();

        CircuitCubesA.Command = new SafeCommand(() => SelectedChannel = 0);
        CircuitCubesB.Command = new SafeCommand(() => SelectedChannel = 1);
        CircuitCubesC.Command = new SafeCommand(() => SelectedChannel = 2);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        CircuitCubesA.SelectedChannel = channel;
        CircuitCubesB.SelectedChannel = channel;
        CircuitCubesC.SelectedChannel = channel;
    }
}
