using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using System.Collections.Generic;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class CreationPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;
        private readonly IPlayLogic _playLogic;
        private readonly IInputDeviceManagerService _inputs;
        private readonly ISharingManager<ControllerProfile> _sharingManagerProfile;

        public CreationPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService,
            IPlayLogic playLogic,
            IInputDeviceManagerService inputs,
            ISharingManager<ControllerProfile> sharingManagerProfile,
            ICommandFactory<Creation> commandFactory,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;
            SharedFileStorageService = sharedFileStorageService;
            _playLogic = playLogic;
            _inputs = inputs;
            _sharingManagerProfile = sharingManagerProfile;
            Creation = parameters.Get<Creation>("creation");

            ImportControllerProfileCommand = new SafeCommand(async () => await ImportControllerProfileAsync(), () => SharedFileStorageService.IsSharedStorageAvailable);
            CopyControllerProfileCommand = new SafeCommand<ControllerProfile>(profile => _sharingManagerProfile.ShareToClipboardAsync(profile));
            PasteControllerProfileCommand = new SafeCommand(PasteControllerProfileAsync);
            ExportCreationCommand = commandFactory.ExportItemAsFileCommand(this, Creation);
            CopyCreationCommand = commandFactory.ShareToClipboardCommand(this, Creation);
            RenameCreationCommand = new SafeCommand(async () => await RenameCreationAsync());
            ShareCreationCommand = new SafeCommand(ShareCreationAsync);
            ShareCreationAsFileCommand = commandFactory.ShareAsJsonFileCommand(this, Creation);
            PlayCommand = new SafeCommand(async () => await PlayAsync());
            AssignControllerCommand = new SafeCommand(AssignControllerAsync);

            AddControllerProfileCommand = new SafeCommand(async () => await AddControllerProfileAsync());
            ControllerProfileTappedCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile))));
            DeleteControllerProfileCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await DeleteControllerProfileAsync(controllerProfile));
            PlayControllerProfileCommand = new SafeCommand<ControllerProfile>(PlayAsync);
        }

        public Creation Creation { get; }
        public string ControllerAssignmentText => Translate("ControllerAssignment") + ": " +
            (Creation.ControllerAssignmentId == null ? Translate("AnyController") :
             Creation.ControllerAssignmentId == "none" ? Translate("NoController") : Creation.ControllerAssignmentName);
        public ICommand AssignControllerCommand { get; }


        private async Task AssignControllerAsync()
        {
            // A listener keeps input discovery alive while the chooser is open.
            void IgnoreInput(object? sender, InputDeviceEventArgs e) { }
            _inputs.InputDeviceEvent += IgnoreInput;
            try
            {
                var devices = _inputs.GetInputDevices();
                var choices = new Dictionary<string, (string? Id, string? Name, string? RuntimeId)>
                {
                    [Translate("AnyController")] = (null, null, null),
                    [Translate("NoController")] = ("none", null, null)
                };
                var controllerIndex = 0;
                foreach (var device in devices)
                {
                    controllerIndex++;
                    var label = devices.Count(d => d.Name == device.Name) > 1
                        ? $"{device.Name} · {device.InputDeviceId}" : device.Name;
                    if (choices.ContainsKey(label)) label += $" (#{controllerIndex})";
                    var id = device.AssignmentId != null && devices.Count(d => d.AssignmentId == device.AssignmentId) == 1
                        ? device.AssignmentId : device.RuntimeId;
                    choices[label] = (id, device.Name, device.RuntimeId);
                }
                var result = await _dialogService.ShowSelectionDialogAsync(choices.Keys,
                    Translate("ControllerAssignment"), Translate("Cancel"), DisappearingToken);
                if (!result.IsOk) return;
                var choice = choices[result.SelectedItem];
                if (choice.RuntimeId != null)
                {
                    var remember = Translate("RememberController");
                    var connectionOnly = Translate("ThisConnectionOnly");
                    var modes = choice.Id != choice.RuntimeId ? new[] { remember, connectionOnly } : new[] { connectionOnly };
                    var mode = await _dialogService.ShowSelectionDialogAsync(modes, choice.Name!, Translate("Cancel"), DisappearingToken);
                    if (!mode.IsOk) return;
                    if (mode.SelectedItem == connectionOnly) choice.Id = choice.RuntimeId;
                }
                var currentDevices = _inputs.GetInputDevices();
                if (choice.RuntimeId != null && CreationPlaySession.Resolve(choice.Id, currentDevices) != choice.RuntimeId)
                {
                    await _dialogService.ShowMessageBoxAsync(Translate("Warning"), Translate("ControllerUnavailable"),
                        Translate("Ok"), DisappearingToken);
                    return;
                }
                if (choice.Id != null && choice.Id != "none" && _creationManager.Creations.Any(c =>
                    c != Creation && (c.ControllerAssignmentId == choice.Id ||
                    (choice.RuntimeId != null && CreationPlaySession.Resolve(c.ControllerAssignmentId, currentDevices) == choice.RuntimeId))))
                {
                    await _dialogService.ShowMessageBoxAsync(Translate("Warning"), Translate("ControllerAlreadyAssigned"),
                        Translate("Ok"), DisappearingToken);
                    return;
                }
                await _creationManager.AssignControllerAsync(Creation, choice.Id, choice.Name);
                RaisePropertyChanged(nameof(ControllerAssignmentText));
            }
            catch (OperationCanceledException) { }
            finally { _inputs.InputDeviceEvent -= IgnoreInput; }
        }

        public bool HasMultipleControllerProfiles => Creation.ControllerProfiles.Count > 1;

        public ISharedFileStorageService SharedFileStorageService { get; }
        public ICommand ImportControllerProfileCommand { get; }
        public ICommand CopyControllerProfileCommand { get; }
        public ICommand PasteControllerProfileCommand { get; }
        public ICommand ExportCreationCommand { get; }
        public ICommand CopyCreationCommand { get; }
        public ICommand ShareCreationCommand { get; }
        public ICommand ShareCreationAsFileCommand { get; }
        public ICommand RenameCreationCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand AddControllerProfileCommand { get; }
        public ICommand ControllerProfileTappedCommand { get; }
        public ICommand DeleteControllerProfileCommand { get; }
        public ICommand PlayControllerProfileCommand { get; }

        private async Task RenameCreationAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Creation.Name,
                    Translate("CreationName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (creationName) => !string.IsNullOrEmpty(creationName),
                    DisappearingToken);
                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("CreationNameCanNotBeEmpty"),
                            Translate("Ok"),
                            DisappearingToken);
                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.RenameCreationAsync(Creation, result.Result),
                        Translate("Renaming"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PlayAsync(ControllerProfile? controllerProfile = default!)
        {
            try
            {
                var validationResult = _playLogic.ValidateCreation(Creation);

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

                    case CreationValidationResult.MissingMacro:
                        warning = Translate("MissingMacro");
                        break;
                }

                if (validationResult == CreationValidationResult.Ok)
                {
                    await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(
                        ("creation", Creation),
                        ("profile", controllerProfile!)));
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
            catch (OperationCanceledException)
            {
            }
        }

        private async Task AddControllerProfileAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    string.Empty,
                    Translate("ProfileName"),
                    Translate("Create"),
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

                    ControllerProfile? controllerProfile = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => controllerProfile = await _creationManager.AddControllerProfileAsync(Creation, result.Result),
                        Translate("Creating"),
                        token: DisappearingToken);
                    // notify profile count change
                    RaisePropertyChanged(nameof(HasMultipleControllerProfiles));

                    await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile!)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteProfile")} '{controllerProfile.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    DisappearingToken))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteControllerProfileAsync(controllerProfile),
                        Translate("Deleting"),
                        token: DisappearingToken);
                    // notify profile count change
                    RaisePropertyChanged(nameof(HasMultipleControllerProfiles));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ImportControllerProfileAsync()
        {
            try
            {
                var controllerProfileFilesMap = FileHelper.EnumerateDirectoryFilesToFilenameMap(SharedFileStorageService.SharedStorageDirectory!, $"*.{FileHelper.ControllerProfileFileExtension}");
                if (controllerProfileFilesMap?.Any() ?? false)
                {
                    var result = await _dialogService.ShowSelectionDialogAsync(
                        controllerProfileFilesMap.Keys,
                        Translate("ControllerProfile"),
                        Translate("Cancel"),
                        DisappearingToken);

                    if (result.IsOk)
                    {
                        try
                        {
                            await _creationManager.ImportControllerProfileAsync(Creation, controllerProfileFilesMap[result.SelectedItem]);
                        }
                        catch (Exception)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Error"),
                                Translate("FailedToImportControllerProfile"),
                                Translate("Ok"),
                                DisappearingToken);
                        }
                    }
                }
                else
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Information"),
                        Translate("NoProfilesToImport"),
                        Translate("Ok"),
                        DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        private async Task PasteControllerProfileAsync()
        {
            try
            {
                var profile = await _sharingManagerProfile.ImportFromClipboardAsync();
                await _creationManager.ImportControllerProfileAsync(Creation, profile);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Error"),
                    Translate("FailedToImportControllerProfile", ex),
                    Translate("Ok"),
                    DisappearingToken);
            }
        }

        private async Task ShareCreationAsync()
        {
            try
            {
                await NavigationService.NavigateToAsync<CreationSharePageViewModel>(new NavigationParameters(("item", Creation)));
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
