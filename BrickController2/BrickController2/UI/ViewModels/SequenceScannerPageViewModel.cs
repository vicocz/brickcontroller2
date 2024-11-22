using BrickController2.CreationManagement;
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

public class SequenceScannerPageViewModel : ScannerPageViewModelBase<Sequence>
{
    private readonly ICreationManager _creationManager;

    public SequenceScannerPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        ISharingManager<Sequence> sharingManager,
        IDialogService dialogService,
        NavigationParameters parameters)
        : base(navigationService, translationService, sharingManager, dialogService, parameters)
    {
        _creationManager = creationManager;
    }

    protected override string ImportFailureWarning => "FailedToImportCreation";

    protected override Task ImportItemAsync(Sequence model)
        => _creationManager.ImportSequenceAsync(model);
}
