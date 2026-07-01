using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BuWizz3ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public BuWizz3ChannelSelectorView()
    {
        InitializeComponent();
        RegisterChannelButtons(BuWizz3Channel0, BuWizz3Channel1, BuWizz3Channel2, BuWizz3Channel3, BuWizz3Channel4, BuWizz3Channel5);
    }
}
