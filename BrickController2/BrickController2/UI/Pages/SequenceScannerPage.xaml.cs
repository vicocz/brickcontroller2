using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.Controls.Xaml;
using ZXing.Net.Maui;

namespace BrickController2.UI.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SequenceScannerPage
{
    public SequenceScannerPage(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost)
        : base(backgroundService, dialogServerHost)
    {
        InitializeComponent();
        AfterInitialize(vm);
    }

    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (BindingContext is SequenceScannerPageViewModel viewModel)
        {
            viewModel.OnBarcodeDetected(e.Results);
        }
    }
}