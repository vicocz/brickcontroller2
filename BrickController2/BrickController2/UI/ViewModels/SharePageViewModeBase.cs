using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels;

public abstract class SharePageViewModeBase<TModel> : PageViewModelBase where TModel : class, IShareable
{
    private string? _barcodeValue;

    private readonly ISharingManager<TModel> _sharingManager;

    protected SharePageViewModeBase(
        INavigationService navigationService,
        ITranslationService translationService,
        ISharingManager<TModel> sharingManager,
        ICommandFactory<TModel> commandFactory,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        _sharingManager = sharingManager;
        Item = parameters.Get<TModel>("item");

        ShareItemCommand = commandFactory.ShareAsTextCommand(this, Item);
        ShareItemAsFileCommand = commandFactory.ShareAsJsonFileCommand(this, Item);
        ExportItemCommand = commandFactory.ExportItemAsFileCommand(this, Item);
        CopyItemCommand = commandFactory.ShareToClipboardCommand(this, Item);
    }

    public TModel Item { get; }

    public ICommand ShareItemCommand { get; }
    public ICommand ShareItemAsFileCommand { get; }
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
        base.OnAppearing();
        // build JSON payload
        BarcodeValue = await _sharingManager.ShareAsync(Item);
    }
}
