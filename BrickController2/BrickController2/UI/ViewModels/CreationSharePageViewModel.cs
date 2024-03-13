using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels;

public class CreationSharePageViewModel : PageViewModelBase
{
    private readonly ICreationManager _creationManager;
    private readonly ISharingManager<Creation> _sharingManager;
    private readonly IDialogService _dialogService;
    private readonly ISharedFileStorageService _sharedFileStorageService;
    private string _barcodeValue;
    private CancellationTokenSource _disappearingTokenSource;

    public CreationSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        ISharingManager<Creation> sharingManager,
        IDialogService dialogService,
        ISharedFileStorageService sharedFileStorageService,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        _creationManager = creationManager;
        _sharingManager = sharingManager;
        _dialogService = dialogService;
        _sharedFileStorageService = sharedFileStorageService;

        Item = parameters.Get<Creation>("item");

        ExportCreationCommand = new SafeCommand(ExportAsync, () => _sharedFileStorageService.IsSharedStorageAvailable);
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
        _disappearingTokenSource?.Cancel();
        _disappearingTokenSource = new CancellationTokenSource();
        // build JSON payload
        BarcodeValue = await _sharingManager.ShareAsync(Item);
    }

    public override void OnDisappearing()
    {
        _disappearingTokenSource.Cancel();
    }

    private async Task ExportAsync()
    {
        try
        {
            var filename = Item.Name;
            var done = false;

            do
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    filename,
                    Translate("CreationName"),
                    Translate("Ok"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    fn => FileHelper.FilenameValidator(fn),
                    _disappearingTokenSource.Token);

                if (!result.IsOk)
                {
                    return;
                }

                filename = result.Result;
                var filePath = Path.Combine(_sharedFileStorageService.SharedStorageDirectory, $"{filename}.{FileHelper.CreationFileExtension}");

                if (!File.Exists(filePath) ||
                    await _dialogService.ShowQuestionDialogAsync(
                        Translate("FileAlreadyExists"),
                        Translate("DoYouWantToOverWrite"),
                        Translate("Yes"),
                        Translate("No"),
                        _disappearingTokenSource.Token))
                {
                    try
                    {
                        await _creationManager.ExportCreationAsync(Item, filePath);
                        done = true;

                        await _dialogService.ShowMessageBoxAsync(
                            Translate("ExportSuccessful"),
                            filePath,
                            Translate("Ok"),
                            _disappearingTokenSource.Token);
                    }
                    catch (Exception)
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Error"),
                            Translate("FailedToExportCreation"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }
                }
            }
            while (!done);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task CopyAsync()
    {
        await _sharingManager.ShareToClipboardAsync(Item);
    }
}
