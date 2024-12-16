using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading.Tasks;

namespace BrickController2.UI.Commands;

internal class CreationCommandFactory : ItemCommandFactoryBase<Creation>, ICommandFactory<Creation>
{
    private readonly ICreationManager _creationManager;

    public CreationCommandFactory
    (
        IDialogService dialogService,
        ITranslationService translationService,
        ISharingManager<Creation> sharingManager,
        ISharedFileStorageService sharedFileStorageService,
        INavigationService navigationService,
        ICreationManager creationManager
    ) : base(dialogService, translationService, sharingManager, sharedFileStorageService, navigationService)
    {
        _creationManager = creationManager;
    }

    protected override string ItemsTitle => Translate("Creations");
    protected override string ItemNameHint => Translate("CreationName");
    protected override string NoItemToImportMessage => Translate("NoCreationsToImport");
    protected override string GetExportFailureDescription(Exception ex) => Translate("FailedToExportCreation", ex);
    protected override string GetImportFailureDescription(Exception ex) => Translate("FailedToImportCreation", ex);

    protected override Task ExportItemAsync(Creation model, string fileName)
        => _creationManager.ExportCreationAsync(model, fileName);

    protected override Task ImportItemAsync(Creation model)
        => _creationManager.ImportCreationAsync(model);
}
