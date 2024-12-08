using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading.Tasks;

namespace BrickController2.UI.Commands;

internal class SequenceCommandFactory : ItemCommandFactoryBase<Sequence>
{
    private readonly ICreationManager _creationManager;

    public SequenceCommandFactory
    (
        IDialogService dialogService,
        ITranslationService translationService,
        ISharingManager<Sequence> sharingManager,
        ISharedFileStorageService sharedFileStorageService,
        INavigationService navigationService,
        ICreationManager creationManager
    ) : base(dialogService, translationService, sharingManager, sharedFileStorageService, navigationService)
    {
        _creationManager = creationManager;
    }

    protected override string ItemsTitle => Translate("Sequences");
    protected override string ItemNameHint => Translate("SequenceName");
    protected override string GetExportFailureDescription(Exception ex) => Translate("FailedToExportSequence", ex);
    protected override string GetImportFailureDescription(Exception ex) => Translate("FailedToImportSequence", ex);

    protected override Task ExportItemAsync(Sequence model, string fileName)
        => _creationManager.ExportSequenceAsync(model, fileName);

    protected override Task ImportItemAsync(Sequence model)
        => _creationManager.ImportSequenceAsync(model);

}
