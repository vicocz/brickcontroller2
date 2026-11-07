using BrickController2.CreationManagement;
using BrickController2.PlatformServices.Permission;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels
{
    public class SequenceListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;
        private readonly ICameraPermission _cameraPermission;

        // Permission request fires OnDisappearing / OnAppearing, so suppress lifecycle transitions meanwhile
        private bool _isRequestingPermission = false;

        public SequenceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            ICameraPermission cameraPermission,
            ICommandFactory<Sequence> commandFactory)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;
            _cameraPermission = cameraPermission;
            ImportSequenceCommand = commandFactory.ImportItemFromFileCommand(this);
            ImportSequenceFromFileCommand = commandFactory.ImportItemFromJsonFileCommand(this);
            ScanSequenceCommand = new SafeCommand(ScanSequenceAsync, () => BarcodeScanning.IsSupported);
            PasteSequenceCommand = commandFactory.PasteItemFromClipboardCommand(this);
            AddSequenceCommand = new SafeCommand(async () => await AddSequenceAsync());
            ShareSequenceCommand = new SafeCommand<Sequence>(async sequence => await NavigationService.NavigateToAsync<SequenceSharePageViewModel>(new NavigationParameters(("item", sequence))));
            SequenceTappedCommand = new SafeCommand<Sequence>(async sequence => await NavigationService.NavigateToAsync<SequenceEditorPageViewModel>(new NavigationParameters(("sequence", sequence))));
            DeleteSequenceCommand = new SafeCommand<Sequence>(async (sequence) => await DeleteSequenceAsync(sequence));
        }

        public ObservableCollection<Sequence> Sequences => _creationManager.Sequences;

        public ICommand ImportSequenceCommand { get; }
        public ICommand ImportSequenceFromFileCommand { get; }
        public ICommand ScanSequenceCommand { get; }
        public ICommand PasteSequenceCommand { get; }
        public ICommand AddSequenceCommand { get; }
        public ICommand ShareSequenceCommand { get; }
        public ICommand SequenceTappedCommand { get; }
        public ICommand DeleteSequenceCommand { get; }


        public override void OnAppearing()
        {
            if (!_isRequestingPermission)
            {
                base.OnAppearing();
            }
        }

        public override void OnDisappearing()
        {
            if (!_isRequestingPermission)
            {
                base.OnDisappearing();
            }
        }

        private async Task AddSequenceAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    string.Empty,
                    Translate("SequenceName"),
                    Translate("Create"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (sequenceName) => !string.IsNullOrEmpty(sequenceName),
                    DisappearingToken);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("SequenceNameCanNotBeEmpty"),
                            Translate("Ok"),
                            DisappearingToken);

                        return;
                    }
                    else if (!(await _creationManager.IsSequenceNameAvailableAsync(result.Result)))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("SequenceNameIsUsed"),
                            Translate("Ok"),
                            DisappearingToken);

                        return;
                    }

                    Sequence? sequence = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            sequence = await _creationManager.AddSequenceAsync(result.Result);
                        },
                        Translate("Creating"),
                        token: DisappearingToken);

                    await NavigationService.NavigateToAsync<SequenceEditorPageViewModel>(new NavigationParameters(("sequence", sequence!)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }


        private async Task ScanSequenceAsync()
        {
            try
            {
                var cameraPermissionStatus = await _cameraPermission.CheckStatusAsync();
                if (cameraPermissionStatus != PermissionStatus.Granted)
                {
                    _isRequestingPermission = true;
                    try
                    {
                        cameraPermissionStatus = await _cameraPermission.RequestAsync();
                    }
                    finally
                    {
                        _isRequestingPermission = false;
                    }

                    DisappearingToken.ThrowIfCancellationRequested();
                }

                if (cameraPermissionStatus != PermissionStatus.Granted)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("CameraWillNOTBeAvailable"),
                        Translate("Ok"),
                        DisappearingToken);
                    DisappearingToken.ThrowIfCancellationRequested();
                }
                else
                {
                    await NavigationService.NavigateToAsync<SequenceScannerPageViewModel>(new NavigationParameters());
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteSequenceAsync(Sequence sequence)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteSequence")} '{sequence.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    DisappearingToken))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteSequenceAsync(sequence),
                        Translate("Deleting"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
