using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

internal class SequenceCommandFactory : ItemCommandFactoryBase<Sequence>, ICommandFactory<Sequence>
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

    public ICommand CreateShareToClipboardCommand(Sequence item)
        => new SafeCommand(() => ShareToClipboardAsync(item));
    public ICommand CreateShareAsJsonFileCommand(Sequence item)
        => new SafeCommand(() => ShareAsJsonFileAsync(item));
    public ICommand CreateShareAsTextCommand(Sequence item)
        => new SafeCommand(() => ShareAsTextAsync(item));
    public ICommand CreateExportItemAsFileCommand(Sequence item, CancellationToken token)
        => new SafeCommand(() => ExportItemAsync(item, token), () => SharedFileStorageService.IsSharedStorageAvailable);
    public ICommand CreateNavigateToSharePageCommand(Sequence creation)
        => new SafeCommand(() => NavigateToItemSharePageAsync<SequenceSharePageViewModel>(creation));

    protected override Task ExportItemAsync(Sequence model, string fileName)
        => _creationManager.ExportSequenceAsync(model, fileName);

    protected override string GetItemDescription(Sequence item) => Translate("SequenceName");

    protected override string GetFailureDescription(Exception ex) => Translate("FailedToExportSequence", ex);
}
