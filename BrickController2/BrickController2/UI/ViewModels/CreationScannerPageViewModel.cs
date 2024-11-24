using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels;

public class CreationScannerPageViewModel : ScannerPageViewModelBase
{
    private readonly ISharingManager<Creation> _sharingManager;
    private readonly ICreationManager _creationManager;

    public CreationScannerPageViewModel(
        ISharingManager<Creation> sharingManager,
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        IDialogService dialogService,
        NavigationParameters parameters)
        : base(navigationService, translationService, dialogService, parameters)
    {
        _sharingManager = sharingManager;
        _creationManager = creationManager;
    }

    protected override IShareable ValidateQrCode(string qr)
        // try to import in order to validate
        => _sharingManager.Import(qr);

    protected override async Task<IShareable> ImportQrCodeAsync(string qr)
    {
        var creation = _sharingManager.Import(qr);
        await _creationManager.ImportCreationAsync(creation);

        return creation;
    }
    protected override string DescribeFailure(Exception ex) => Translate("FailedToImportCreation", ex);
}
