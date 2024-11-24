using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels;

public class CreationSharePageViewModel : SharePageViewModeBase<Creation>
{
    private readonly ICreationManager _creationManager;

    public CreationSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        ISharingManager<Creation> sharingManager,
        IDialogService dialogService,
        ISharedFileStorageService sharedFileStorageService,
        NavigationParameters parameters)
        : base(navigationService, translationService, sharingManager, dialogService, sharedFileStorageService, parameters)
    {
        _creationManager = creationManager;
    }

    protected override Task ExportItemAsync(Creation model, string fileName)
        => _creationManager.ExportCreationAsync(model, fileName);

    protected override string DescribeItem(Creation item) => Translate("CreationName");

    protected override string DescribeFailure(Exception ex) => Translate("FailedToExportCreation", ex);
}
