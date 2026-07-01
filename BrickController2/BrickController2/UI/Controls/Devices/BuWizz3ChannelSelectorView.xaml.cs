using BrickController2.UI.Commands;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls.Devices;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BuWizz3ChannelSelectorView : DeviceChannelSelectorViewBase
{
    public BuWizz3ChannelSelectorView()
    {
        InitializeComponent();

        BuWizz3Channel0.Command = new SafeCommand(() => SelectedChannel = 0);
        BuWizz3Channel1.Command = new SafeCommand(() => SelectedChannel = 1);
        BuWizz3Channel2.Command = new SafeCommand(() => SelectedChannel = 2);
        BuWizz3Channel3.Command = new SafeCommand(() => SelectedChannel = 3);
        BuWizz3Channel4.Command = new SafeCommand(() => SelectedChannel = 4);
        BuWizz3Channel5.Command = new SafeCommand(() => SelectedChannel = 5);
    }

    protected override void OnSelectedChannelChanged(int channel)
    {
        BuWizz3Channel0.SelectedChannel = channel;
        BuWizz3Channel1.SelectedChannel = channel;
        BuWizz3Channel2.SelectedChannel = channel;
        BuWizz3Channel3.SelectedChannel = channel;
        BuWizz3Channel4.SelectedChannel = channel;
        BuWizz3Channel5.SelectedChannel = channel;
    }
}
