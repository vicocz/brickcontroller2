using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class CreationPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;
        private readonly IPlayLogic _playLogic;
        private readonly ISharingManager<ControllerProfile> _sharingManagerProfile;
        private readonly IDeviceManager _deviceManager;

        public CreationPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService,
            IPlayLogic playLogic,
            ISharingManager<ControllerProfile> sharingManagerProfile,
            ICommandFactory<Creation> commandFactory,
            IDeviceManager deviceManager,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;
            SharedFileStorageService = sharedFileStorageService;
            _playLogic = playLogic;
            _sharingManagerProfile = sharingManagerProfile;
            _deviceManager = deviceManager;
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
            FixItCommand = new SafeCommand(async () => await FixItAsync());
            AddControllerProfileCommand = new SafeCommand(async () => await AddControllerProfileAsync());
            ControllerProfileTappedCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile))));
            DeleteControllerProfileCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await DeleteControllerProfileAsync(controllerProfile));
            PlayControllerProfileCommand = new SafeCommand<ControllerProfile>(PlayAsync);
        }

        public Creation Creation { get; }

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
        public ICommand FixItCommand { get; }
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

        private readonly struct DeviceModel(Device device)
        {
            public string DeviceId => device.Id;
            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(device.Name) ? device.Address : device.Name;
            }
        }

        private async Task FixItAsync()
        {
            try
            {
                var validationResult = _playLogic.ValidateCreation(Creation);

                if (validationResult == CreationValidationResult.MissingDevice)
                {
                    // get missing device IDs
                    var deviceIds = _playLogic.GetMissingDevices(Creation)
                        .Select(id =>
                        {
                            DeviceId.TryParse(id, out var deviceType, out var deviceAddress);
                            return (DeviceType: deviceType, Address: deviceAddress);
                        })
                        .Where(x => x.DeviceType != DeviceType.Unknown && x.Address != null)
                        .ToList();

                    // get missing types
                    var missingTypes = deviceIds.Select(x => x.DeviceType).ToHashSet();

                    if (missingTypes.Count == 0 || _deviceManager.Devices.All(x => !missingTypes.Contains(x.DeviceType)))
                    {
                        // report error - no suitable missing devices
                        return;
                    }

                    DeviceType deviceType;

                    if (missingTypes.Count == 1)
                    {
                        deviceType = missingTypes.First();
                    }
                    else
                    {
                        // choose device type to remap
                        var sourceType = await _dialogService.ShowSelectionDialogAsync(
                            missingTypes.Select(x => x.ToString()),
                            Translate("Missing device type"),
                            Translate("Cancel"),
                            DisappearingToken);

                        if (!sourceType.IsOk || !Enum.TryParse(sourceType.SelectedItem, out deviceType))
                        {
                            return; // user cancelled or invalid type
                        }
                    }

                    // have device type
                    var missingDeviceAddresses = deviceIds
                        .Where(x => x.DeviceType == deviceType)
                        .Select(x => x.Address!)
                        .ToList();

                    string missingDeviceAddress = missingDeviceAddresses[0];
                    if (missingDeviceAddresses.Count > 1)
                    {
                        var sourceDevice = await _dialogService.ShowSelectionDialogAsync(
                            missingDeviceAddresses,
                            Translate("Missing device"),
                            Translate("Cancel"),
                            DisappearingToken);

                        if (!sourceDevice.IsOk)
                        {
                            return; // user cancelled
                        }

                        missingDeviceAddress = sourceDevice.SelectedItem!;
                    }

                    var suitableDevices = _deviceManager.Devices
                        .Where(d => d.DeviceType == deviceType)
                        .Select(d => new DeviceModel(d))
                        .ToList();

                    var targetDevice = await _dialogService.ShowSelectionDialogAsync(
                        suitableDevices,
                        Translate("Target device"),
                        Translate("Cancel"),
                        DisappearingToken);

                    if (targetDevice.IsOk)
                    {
                        // replace all missing device IDs with the selected one
                        var missingDeviceId = DeviceId.Get(deviceType, missingDeviceAddress);
                        var newDeviceId = targetDevice.SelectedItem!.DeviceId;
                        var count = _playLogic.RemapDevice(Creation, missingDeviceId, newDeviceId);

                        //TODO update creation

                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Information"),
                            Translate($"Remapped {count} controller actions from device '{missingDeviceId}' to '{newDeviceId}'"),
                            Translate("Ok"),
                            DisappearingToken);
                    }
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
