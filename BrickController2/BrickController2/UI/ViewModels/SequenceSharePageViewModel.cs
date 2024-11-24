using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels;

public class SequenceSharePageViewModel : SharePageViewModeBase<Sequence>
{
    private readonly ICreationManager _creationManager;

    public SequenceSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        ISharingManager<Sequence> sharingManager,
        IDialogService dialogService,
        ISharedFileStorageService sharedFileStorageService,
        NavigationParameters parameters)
        : base(navigationService, translationService, sharingManager, dialogService, sharedFileStorageService, parameters)
    {
        _creationManager = creationManager;
    }

    protected override Task ExportItemAsync(Sequence model, string fileName)
        => _creationManager.ExportSequenceAsync(model, fileName);

    protected override string DescribeItem(Sequence item) => Translate("SequenceName");

    protected override string DescribeFailure(Exception ex) => Translate("FailedToExportSequence", ex);
}
