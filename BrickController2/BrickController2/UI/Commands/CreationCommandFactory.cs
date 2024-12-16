using BrickController2.BusinessLogic;
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

internal class CreationCommandFactory : ItemCommandFactoryBase<Creation>, ICreationCommandFactory
{
    private readonly ICreationManager _creationManager;
    private readonly IPlayLogic _playLogic;

    public CreationCommandFactory
    (
        IDialogService dialogService,
        ITranslationService translationService,
        ISharingManager<Creation> sharingManager,
        ISharedFileStorageService sharedFileStorageService,
        INavigationService navigationService,
        ICreationManager creationManager,
        IPlayLogic playLogic
    ) : base(dialogService, translationService, sharingManager, sharedFileStorageService, navigationService)
    {
        _creationManager = creationManager;
        _playLogic = playLogic;
    }
    public ICommand PlayCommand(PageViewModelBase viewModel, Creation item, ControllerProfile? profile)
        => new SafeCommand(() => PlayAsync(item, profile, viewModel.DisappearingToken));

    public ICommand PlayCommand(PageViewModelBase viewModel)
        => new SafeCommand<Creation>((creation) => PlayAsync(creation, default, viewModel.DisappearingToken));

    public ICommand PlayProfileCommand(PageViewModelBase viewModel)
        => new SafeCommand<ControllerProfile>((profile) => PlayAsync(profile.Creation!, profile, viewModel.DisappearingToken));

    protected override string ItemsTitle => Translate("Creations");
    protected override string ItemNameHint => Translate("CreationName");
    protected override string NoItemToImportMessage => Translate("NoCreationsToImport");
    protected override string GetExportFailureDescription(Exception ex) => Translate("FailedToExportCreation", ex);
    protected override string GetImportFailureDescription(Exception ex) => Translate("FailedToImportCreation", ex);

    protected override Task ExportItemAsync(Creation model, string fileName)
        => _creationManager.ExportCreationAsync(model, fileName);

    protected override Task ImportItemAsync(Creation model)
        => _creationManager.ImportCreationAsync(model);

    private async Task PlayAsync(Creation creation, ControllerProfile? profile, CancellationToken token)
    {
        try
        {
            var validationResult = _playLogic.ValidateCreation(creation);

            string warning = string.Empty;
            switch (validationResult)
            {
                case CreationValidationResult.MissingControllerAction:
                    warning = Translate("NoControllerActions");
                    break;

                case CreationValidationResult.MissingDevice:
                    warning = Translate("MissingDevices");
                    break;

                case CreationValidationResult.MissingSequence:
                    warning = Translate("MissingSequence");
                    break;
            }

            if (validationResult == CreationValidationResult.Ok)
            {
                await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(("creation", creation), ("profile", profile)));
            }
            else
            {
                await DialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("Play") + $" '{creation.Name}': {warning}",
                    Translate("Ok"),
                    token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
