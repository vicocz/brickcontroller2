using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

internal class CreationCommandFactory : ItemCommandFactoryBase<Creation>, ICommandFactory<Creation>, ICreationCommandFactory
{
    private readonly ICreationManager _creationManager;
    private readonly IDeviceManager _deviceManager;
    private readonly IPlayLogic _playLogic;

    public CreationCommandFactory
    (
        IDialogService dialogService,
        ITranslationService translationService,
        ISharingManager<Creation> sharingManager,
        ISharedFileStorageService sharedFileStorageService,
        INavigationService navigationService,
        ICreationManager creationManager,
        IDeviceManager deviceManager,
        IPlayLogic playLogic
    ) : base(dialogService, translationService, sharingManager, sharedFileStorageService, navigationService)
    {
        _creationManager = creationManager;
        _deviceManager = deviceManager;
        _playLogic = playLogic;
    }

    public ICommand PlayCommand(PageViewModelBase viewModel, Creation creation, ControllerProfile? controllerProfile = default!)
        => new SafeCommand(() => PlayAsync(viewModel, creation, controllerProfile));

    public ICommand PlayCreationCommand(PageViewModelBase viewModel)
        => new SafeCommand<Creation>((creation) => PlayAsync(viewModel, creation));

    public ICommand PlayControllerProfileCommand(PageViewModelBase viewModel)
    => new SafeCommand<ControllerProfile>((profile) => PlayAsync(viewModel, profile.Creation, profile));

    public ICommand FixCommand(PageViewModelBase viewModel, Creation creation)
        => new SafeCommand(() => FixItAsync(viewModel, creation));

    protected override string ItemsTitle => Translate("Creations");
    protected override string ItemNameHint => Translate("CreationName");
    protected override string NoItemToImportMessage => Translate("NoCreationsToImport");
    protected override string GetExportFailureDescription(Exception ex) => Translate("FailedToExportCreation", ex);
    protected override string GetImportFailureDescription(Exception ex) => Translate("FailedToImportCreation", ex);

    protected override Task ExportItemAsync(Creation model, string fileName)
        => _creationManager.ExportCreationAsync(model, fileName);

    protected override Task ImportItemAsync(Creation model)
        => _creationManager.ImportCreationAsync(model);

    private async Task PlayAsync(PageViewModelBase viewModel, Creation creation, ControllerProfile? controllerProfile = default!)
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
                await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(
                    ("creation", creation),
                    ("profile", controllerProfile)));
            }
            else
            {
                await DialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    warning,
                    Translate("Ok"),
                    viewModel.DisappearingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task FixItAsync(PageViewModelBase viewModel, Creation creation)
    {
        try
        {
            var validationResult = _playLogic.ValidateCreation(creation);

            if (validationResult == CreationValidationResult.MissingDevice)
            {
                await FixMissingDevicesAsync(viewModel, creation);
            }
            else if (validationResult == CreationValidationResult.MissingSequence)
            {
                await DialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("MissingSequence"),
                    Translate("Ok"),
                    viewModel.DisappearingToken);
            }
            else if (validationResult == CreationValidationResult.MissingControllerAction)
            {
                await DialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("NoControllerActions"),
                    Translate("Ok"),
                    viewModel.DisappearingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<bool> FixMissingDevicesAsync(PageViewModelBase viewModel, Creation creation)
    {
        // get source device IDs
        var sourceDeviceIds = //TODO _playLogic.GetMissingDevices(creation)
            creation.GetDeviceIds()
            .Select(id =>
            {
                DeviceId.TryParse(id, out var deviceType, out var deviceAddress);
                return (DeviceType: deviceType, Address: deviceAddress);
            })
            .Where(x => x.DeviceType != DeviceType.Unknown && x.Address != null)
            .ToList();

        // get source types
        var sourceTypes = sourceDeviceIds.Select(x => x.DeviceType).ToHashSet();
        var addresses = sourceDeviceIds.Select(x => x.Address!).ToHashSet();
        // source types that have some existing device of such type present, but not used in creation
        var suitableTypes = sourceTypes.Where(x => _deviceManager.Devices
            .Any(d => d.DeviceType == x && !addresses.Contains(d.Address)))
            .ToHashSet();

        if (sourceTypes.Count == 0 || suitableTypes.Count == 0)
        {
            // report error - no suitable device to replace
            await DialogService.ShowMessageBoxAsync(
                Translate("Information"),
                Translate("No suitable device found to remap a device."),
                Translate("Ok"),
                viewModel.DisappearingToken);
            return false;
        }

        var sourceType = await ChooseDeviceTypeToRemapAsync(viewModel, suitableTypes);
        if (sourceType is null)
        {
            // user cancelled
            return false;
        }

        // have source device type, get addresses of such type
        var sourceDeviceAddresses = sourceDeviceIds
            .Where(x => x.DeviceType == sourceType.Value)
            .Select(x => x.Address!)
            .ToList();

        var sourceDeviceAddress = await ChooseDeviceAddressToRemapAsync(viewModel, sourceDeviceAddresses);
        if (sourceDeviceAddress is null)
        {
            // user cancelled
            return false;
        }

        // choose target device by name
        var suitableDevices = _deviceManager.Devices
            .Where(d => d.DeviceType == sourceType.Value)
            .OrderBy(x => x.Name)
            .ToList();

        var targetDevice = await DialogService.ShowSelectionDialogAsync(
            suitableDevices,
            Translate("Target device"),
            Translate("Cancel"),
            viewModel.DisappearingToken);

        if (targetDevice.IsOk)
        {
            // replace all source device IDs with the selected one
            var sourceDeviceId = DeviceId.Get(sourceType.Value, sourceDeviceAddress);
            var newDeviceId = targetDevice.SelectedItem!.Id;
            var count = await _creationManager.RemapDevice(creation, sourceDeviceId, newDeviceId);

            // revalidate creation
            creation.ValidationResult = _playLogic.ValidateCreation(creation);

            await DialogService.ShowMessageBoxAsync(
                Translate("Information"),
                Translate($"Remapped {count} controller actions from device '{sourceDeviceId}' to '{newDeviceId}'"),
                Translate("Ok"),
                viewModel.DisappearingToken);
        }

        return true;
    }

    private async Task<string?> ChooseDeviceAddressToRemapAsync(PageViewModelBase viewModel, List<string> missingDeviceAddresses)
    {
        if (missingDeviceAddresses.Count == 1)
        {
            return missingDeviceAddresses[0];
        }

        // choose address to remap
        var sourceDevice = await DialogService.ShowSelectionDialogAsync(
            missingDeviceAddresses.Order(),
            Translate("Source device"),
            Translate("Cancel"),
            viewModel.DisappearingToken);

        return sourceDevice.IsOk ? sourceDevice.SelectedItem : default;
    }

    private async Task<DeviceType?> ChooseDeviceTypeToRemapAsync(PageViewModelBase viewModel, HashSet<DeviceType> suitableTypes)
    {
        if (suitableTypes.Count == 1)
        {
            return suitableTypes.First();
        }

        // choose device type to remap
        var deviceType = await DialogService.ShowSelectionDialogAsync(
            suitableTypes.OrderBy(x => x.ToString()),
            Translate("Source device type"),
            Translate("Cancel"),
            viewModel.DisappearingToken);

        return deviceType.IsOk ? deviceType.SelectedItem : default;
    }
}
