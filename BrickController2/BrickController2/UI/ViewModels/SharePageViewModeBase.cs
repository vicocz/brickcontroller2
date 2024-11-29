using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels;

public abstract class SharePageViewModeBase<TModel> : PageViewModelBase where TModel : class, IShareable
{
    private readonly IDialogService _dialogService;
    private readonly ISharedFileStorageService _sharedFileStorageService;
    private string? _barcodeValue;
    private CancellationTokenSource? _disappearingTokenSource;

    protected readonly ISharingManager<TModel> SharingManager;

    public SharePageViewModeBase(
        INavigationService navigationService,
        ITranslationService translationService,
        ISharingManager<TModel> sharingManager,
        IDialogService dialogService,
        ISharedFileStorageService sharedFileStorageService,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        SharingManager = sharingManager;
        _dialogService = dialogService;
        _sharedFileStorageService = sharedFileStorageService;

        Item = parameters.Get<TModel>("item");

        ExportItemCommand = new SafeCommand(ExportAsync, () => _sharedFileStorageService.IsSharedStorageAvailable);
        CopyItemCommand = new SafeCommand(CopyAsync);
    }

    public TModel Item { get; }

    public ICommand ExportItemCommand { get; }
    public ICommand CopyItemCommand { get; }

    public BarcodeFormat BarcodeFormat { get; } = BarcodeFormat.QrCode;

    public string? BarcodeValue
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
        BarcodeValue = await SharingManager.ShareAsync(Item);
    }

    public override void OnDisappearing()
    {
        _disappearingTokenSource?.Cancel();
    }

    protected CancellationToken DisappearingToken => _disappearingTokenSource?.Token ?? default;

    protected abstract Task ExportItemAsync(TModel model, string fileName);

    protected abstract string DescribeItem(TModel item);

    protected abstract string DescribeFailure(Exception ex);

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
                    DescribeItem(Item),
                    Translate("Ok"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    fn => FileHelper.FilenameValidator(fn),
                    DisappearingToken);

                if (!result.IsOk)
                {
                    return;
                }

                filename = result.Result;
                var filePath = Path.Combine(_sharedFileStorageService.SharedStorageDirectory!, $"{filename}.{FileHelper.CreationFileExtension}");

                if (!File.Exists(filePath) ||
                    await _dialogService.ShowQuestionDialogAsync(
                        Translate("FileAlreadyExists"),
                        Translate("DoYouWantToOverWrite"),
                        Translate("Yes"),
                        Translate("No"),
                        DisappearingToken))
                {
                    try
                    {
                        await ExportItemAsync(Item, filePath);
                        done = true;

                        await _dialogService.ShowMessageBoxAsync(
                            Translate("ExportSuccessful"),
                            filePath,
                            Translate("Ok"),
                            DisappearingToken);
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Error"),
                            DescribeFailure(ex),
                            Translate("Ok"),
                            DisappearingToken);

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
        await SharingManager.ShareToClipboardAsync(Item);
    }
}
