using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages;

public class SequenceScannerPage : ScannerPageBase
{
    public SequenceScannerPage(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost, IPreferencesService preferencesService)
        : base(vm, backgroundService, dialogServerHost, preferencesService)
    {
    }
}
