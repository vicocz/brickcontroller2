using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Translation;
using BrickController2.BusinessLogic;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.Helpers;
using DeviceType = BrickController2.DeviceManagement.DeviceType;
using BrickController2.CreationManagement.Sharing;
using BrickController2.PlatformServices.GameController;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfilePageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly ISharingManager<ControllerProfile> _sharingManager;
        private readonly IDialogService _dialogService;
        private readonly IPlayLogic _playLogic;
        private readonly IGameControllerService _gameControllerService;

        private List<ControllerEventViewModel> _controllerEvents = new List<ControllerEventViewModel>();

        public ControllerProfilePageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            ISharingManager<ControllerProfile> sharingManager,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService,
            IPlayLogic playLogic,
            IGameControllerService gameControllerService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _sharingManager = sharingManager;
            _dialogService = dialogService;
            SharedFileStorageService = sharedFileStorageService;
            _playLogic = playLogic;
            _gameControllerService = gameControllerService;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");

            ExportControllerProfileCommand = new SafeCommand(async () => await ExportControllerProfileAsync(), () => SharedFileStorageService.IsSharedStorageAvailable);
            CopyControllerProfileCommand = new SafeCommand(CopyControllerProfileAsync);
            RenameProfileCommand = new SafeCommand(async () => await RenameControllerProfileAsync());
            AddControllerEventCommand = new SafeCommand(async () => await AddControllerEventAsync(false));
            AddControllerEventForSpecificControllerIdCommand = new SafeCommand(async () => await AddControllerEventAsync(true));
            PlayCommand = new SafeCommand(async () => await PlayAsync());
            ControllerActionTappedCommand = new SafeCommand<ControllerActionViewModel>(async controllerActionViewModel => await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controlleraction", controllerActionViewModel.ControllerAction))));
            DeleteControllerEventCommand = new SafeCommand<ControllerEvent>(async controllerEvent => await DeleteControllerEventAsync(controllerEvent));
            AddAnotherActionCommand = new SafeCommand<ControllerEvent>(AddAnotherActionAsync);
            DeleteControllerActionCommand = new SafeCommand<ControllerAction>(async controllerAction => await DeleteControllerActionAsync(controllerAction));

            PopulateControllerEvents();
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            PopulateControllerEvents();
        }

        public ControllerProfile ControllerProfile { get; }

        public ISharedFileStorageService SharedFileStorageService { get; }

        public List<ControllerEventViewModel> ControllerEvents
        {
            get { return _controllerEvents; }
            set { _controllerEvents = value; RaisePropertyChanged(); }
        }

        public bool IsControllerIdSupported => _gameControllerService.IsControllerIdSupported;

        public ICommand ExportControllerProfileCommand { get; }
        public ICommand CopyControllerProfileCommand { get; }
        public ICommand RenameProfileCommand { get; }
        public ICommand AddControllerEventCommand { get; }
        public ICommand AddControllerEventForSpecificControllerIdCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand ControllerActionTappedCommand { get; }
        public ICommand DeleteControllerEventCommand { get; }
        public ICommand AddAnotherActionCommand { get; }
        
        public ICommand DeleteControllerActionCommand { get; }

        private void PopulateControllerEvents()
        {
            ControllerEvents = new List<ControllerEventViewModel>(ControllerProfile.ControllerEvents.Select(ce => new ControllerEventViewModel(ce, _deviceManager, _playLogic, TranslationService)));
        }

        private async Task ExportControllerProfileAsync()
        {
            try
            {
                var filename = ControllerProfile.Name;
                var done = false;

                do
                {
                    var result = await _dialogService.ShowInputDialogAsync(
                        filename,
                        Translate("ProfileName"),
                        Translate("Ok"),
                        Translate("Cancel"),
                        KeyboardType.Text,
                        fn => FileHelper.FilenameValidator(fn),
                        DisappearingToken);

                    if (!result.IsOk)
                    {
                        return;
                    }

                    filename = result.Result;
                    var filePath = Path.Combine(SharedFileStorageService.SharedStorageDirectory!, $"{filename}.{FileHelper.ControllerProfileFileExtension}");

                    if (!File.Exists(filePath) ||
                        await _dialogService.ShowQuestionDialogAsync(
                            Translate("FileAlreadyExists"),
                            Translate("DoYouWantToOverWrite"),
                            Translate("Yes"),
                            Translate("No"),
                            DisappearingToken))
                    {
                        try
                        {
                            await _creationManager.ExportControllerProfileAsync(ControllerProfile, filePath);
                            done = true;

                            await _dialogService.ShowMessageBoxAsync(
                                Translate("ExportSuccessful"),
                                filePath,
                                Translate("Ok"),
                                DisappearingToken);
                        }
                        catch (Exception)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Error"),
                                Translate("FailedToExportControllerProfile"),
                                Translate("Ok"),
                                DisappearingToken);

                            return;
                        }
                    }
                }
                while (!done);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private Task CopyControllerProfileAsync()
            => _sharingManager.ShareToClipboardAsync(ControllerProfile);

        private async Task RenameControllerProfileAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    ControllerProfile.Name,
                    Translate("ProfileName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (profileName) => !string.IsNullOrEmpty(profileName),
                    DisappearingToken);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("ProfileNameCanNotBeEmpty"),
                            Translate("Ok"),
                            DisappearingToken);
                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.RenameControllerProfileAsync(ControllerProfile, result.Result),
                        Translate("Renaming"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task AddControllerEventAsync(bool addControllerId)
        {
            try
            {
                if (_deviceManager.Devices?.Count == 0)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ScanForDevicesFirst"),
                        Translate("Ok"),
                        DisappearingToken);
                    return;
                }

                var result = await _dialogService.ShowGameControllerEventDialogAsync(
                    Translate("Controller"),
                    Translate("PressButtonOrMoveJoy"),
                    Translate("Cancel"),
                    DisappearingToken);
                if (result.IsOk)
                {
                    ControllerEvent? controllerEvent = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => controllerEvent = await _creationManager.AddOrGetControllerEventAsync(ControllerProfile, addControllerId ? result.ControllerId : string.Empty, result.EventType, result.EventCode),
                        Translate("Creating"),
                        token: DisappearingToken);

                    await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent!)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PlayAsync()
        {
            var validationResult = _playLogic.ValidateCreation(ControllerProfile.Creation!);

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
                await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(
                  ("creation", ControllerProfile.Creation!),
                  ("profile", ControllerProfile)));
            }
            else
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    warning,
                    Translate("Ok"),
                    DisappearingToken);
            }
        }
        private async Task AddAnotherActionAsync(ControllerEvent controllerEvent)
        {
            try
            {
                if (_deviceManager.Devices?.Count == 0)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ScanForDevicesFirst"),
                        Translate("Ok"),
                        DisappearingToken);
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.AddOrGetControllerEventAsync(ControllerProfile, controllerEvent.ControllerId, controllerEvent.EventType, controllerEvent.EventCode),
                    Translate("Creating"),
                    token: DisappearingToken);

                await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent!)));
                
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteControllerEvent")} {controllerEvent.EventCode}?",
                    Translate("Yes"),
                    Translate("No"),
                    DisappearingToken))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            await _creationManager.DeleteControllerEventAsync(controllerEvent);
                            PopulateControllerEvents();
                        },
                        Translate("Deleting"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    Translate("AreYouSureToDeleteThisControllerAcrion"),
                    Translate("Yes"),
                    Translate("No"),
                    DisappearingToken))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            var controllerEvent = controllerAction.ControllerEvent;
                            await _creationManager.DeleteControllerActionAsync(controllerAction);
                            if (controllerEvent!.ControllerActions.Count == 0)
                            {
                                await _creationManager.DeleteControllerEventAsync(controllerEvent);
                            }

                            PopulateControllerEvents();
                        },
                        Translate("Deleting"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public class ControllerActionViewModel
        {
            public ControllerActionViewModel(
                ControllerAction controllerAction,
                IDeviceManager deviceManager,
                IPlayLogic playLogic,
                ITranslationService translationService)
            {
                ControllerAction = controllerAction;
                var device = deviceManager.GetDeviceById(controllerAction.DeviceId);

                ControllerActionValid = playLogic.ValidateControllerAction(controllerAction);
                DeviceName = device != null ? device.Name : translationService.Translate("Missing");
                DeviceType = device != null ? device.DeviceType : DeviceType.Unknown;
                Channel = controllerAction.Channel;
                InvertName = controllerAction.IsInvert ? translationService.Translate("Inv") : string.Empty;
            }

            public ControllerAction ControllerAction { get; }
            public bool ControllerActionValid { get; }
            public string DeviceName { get; }
            public DeviceType DeviceType { get; }
            public int Channel { get; }
            public string InvertName { get; }
        }

        public class ControllerEventViewModel : List<ControllerActionViewModel>
        {
            public ControllerEventViewModel(
                ControllerEvent controllerEvent,
                IDeviceManager deviceManager,
                IPlayLogic playLogic,
                ITranslationService translationService)
            {
                ControllerEvent = controllerEvent;

                AddRange(controllerEvent.ControllerActions.Select(ca => new ControllerActionViewModel(ca, deviceManager, playLogic, translationService)));
            }

            public ControllerEvent ControllerEvent { get; }
        }
    }
}
