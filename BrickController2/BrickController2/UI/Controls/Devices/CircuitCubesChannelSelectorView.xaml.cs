using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CircuitCubesChannelSelectorView : DeviceChannelSelectorViewBase
{
    public CircuitCubesChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(CircuitCubesA, CircuitCubesB, CircuitCubesC);
    }
}
