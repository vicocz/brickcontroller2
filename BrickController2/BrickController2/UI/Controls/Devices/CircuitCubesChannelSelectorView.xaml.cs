using BrickController2.DeviceManagement;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CircuitCubesChannelSelectorView : DeviceChannelSelectorViewBase, IDeviceChannelSelectorView
{
    public static DeviceType DeviceType => DeviceType.CircuitCubes;
    protected override DeviceType SelectorDeviceType => DeviceType;

    public CircuitCubesChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(CircuitCubesA, CircuitCubesB, CircuitCubesC);
    }
}
