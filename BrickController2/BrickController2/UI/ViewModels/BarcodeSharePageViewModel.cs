using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels;

public class BarcodeSharePageViewModel : PageViewModelBase
{
    private readonly ISharingManager<Creation> _sharingManager;

    private string _barcodeValue;

    public BarcodeSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ISharingManager<Creation> sharingManager,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        _sharingManager = sharingManager;

        Item = parameters.Get<Creation>("item");

        ExportCreationCommand = new SafeCommand(ExportAsync);
        CopyCreationCommand = new SafeCommand(CopyAsync);
    }

    public Creation Item { get; }

    public ICommand ExportCreationCommand { get; }
    public ICommand CopyCreationCommand { get; }

    public BarcodeFormat BarcodeFormat { get; } = BarcodeFormat.QrCode;

    public string BarcodeValue
    {
        get { return _barcodeValue; }
        set
        {
            _barcodeValue = value;
            RaisePropertyChanged();
        }
    }

    public override async void OnAppearing()
    {
        // build JSON payload
        BarcodeValue = await _sharingManager.ShareAsync(Item);
    }

    private async Task ExportAsync()
    { 
        //TODO
    }

    private async Task CopyAsync()
    {
        //TODO
    }
}
