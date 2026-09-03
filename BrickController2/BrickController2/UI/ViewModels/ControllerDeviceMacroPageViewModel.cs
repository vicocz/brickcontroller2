using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Macros;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels;

public class ControllerDeviceMacroPageViewModel : PageViewModelBase
{
    private readonly ICreationManager _creationManager;
    private readonly IDeviceManager _deviceManager;
    private readonly IDialogService _dialogService;

    private Device? _selectedDevice;

    public ControllerDeviceMacroPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        IDeviceManager deviceManager,
        IDialogService dialogService,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        _creationManager = creationManager;
        _deviceManager = deviceManager;
        _dialogService = dialogService;

        ControllerAction = parameters.Get<ControllerAction?>("controlleraction", null);
        ControllerEvent = parameters.Get<ControllerEvent?>("controllerevent", null) ?? ControllerAction?.ControllerEvent!;

        Action.ButtonType = ControllerButtonType.DeviceMacro;

        var device = _deviceManager.GetDeviceById(ControllerAction?.DeviceId);
        if (ControllerAction is not null && device is not null)
        {
            Action.MacroId = ControllerAction.MacroId;
            Action.MacroChoiceValue = ControllerAction.MacroChoiceValue;
        }
        else
        {
            device = DevicesWithDeviceMacros.FirstOrDefault();
            Action.MacroId = string.Empty;
            Action.MacroChoiceValue = null;
        }

        SelectedDevice = device;

        SaveCommand = new SafeCommand(SaveAsync, () => SelectedDevice != null && !string.IsNullOrEmpty(Action.MacroId) && !_dialogService.IsDialogOpen);
        SelectDeviceCommand = new SafeCommand(SelectDeviceAsync, () => DevicesWithDeviceMacros.Count > 0);
        SelectMacroCommand = new SafeCommand(SelectMacroAsync, () => SelectedDevice != null && AvailableMacros.Count > 0);
        SelectMacroChoiceCommand = new SafeCommand(SelectMacroChoiceAsync, () => SelectedMacro != null && SelectedMacro.Choices.Count > 0);
    }

    public ControllerEvent? ControllerEvent { get; }
    public ControllerAction? ControllerAction { get; }
    public ControllerAction Action { get; } = new ControllerAction();

    public ObservableCollection<Device> Devices => _deviceManager.Devices;

    public IReadOnlyList<Device> DevicesWithDeviceMacros
        => Devices.Where(d => d.AvailableMacros.Any(m => m.Scope == MacroScope.Device)).ToList();

    public Device? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            if (value != null)
            {
                Action.DeviceId = value.Id;
                if (!AvailableMacros.Any(m => m.Id == Action.MacroId))
                {
                    var first = AvailableMacros.FirstOrDefault();
                    Action.MacroId = first?.Id ?? string.Empty;
                    Action.MacroChoiceValue = first?.Choices.Count > 0 ? (int)first.Choices[0].BoxedValue : null;
                }
            }
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(AvailableMacros));
            RaisePropertyChanged(nameof(SelectedMacro));
            RaisePropertyChanged(nameof(SelectedMacroDisplayName));
            RaisePropertyChanged(nameof(SelectedMacroChoiceDisplayName));
        }
    }

    public IReadOnlyList<MacroDescriptor> AvailableMacros
        => _selectedDevice?.AvailableMacros.Where(m => m.Scope == MacroScope.Device).ToList() ?? [];

    public MacroDescriptor? SelectedMacro
        => AvailableMacros.FirstOrDefault(m => m.Id == Action.MacroId);

    public string SelectedMacroDisplayName
        => SelectedMacro is null ? string.Empty : Translate(SelectedMacro.NameKey);

    public string SelectedMacroChoiceDisplayName
    {
        get
        {
            var macro = SelectedMacro;
            if (macro is null || Action.MacroChoiceValue is null)
            {
                return string.Empty;
            }
            var choice = macro.Choices.FirstOrDefault(c => c.BoxedValue == Action.MacroChoiceValue);
            return choice is null ? string.Empty : Translate(choice.LabelKey);
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand SelectDeviceCommand { get; }
    public ICommand SelectMacroCommand { get; }
    public ICommand SelectMacroChoiceCommand { get; }

    private async Task SaveAsync()
    {
        if (SelectedDevice == null || string.IsNullOrEmpty(Action.MacroId))
        {
            await _dialogService.ShowMessageBoxAsync(
                Translate("Warning"),
                Translate("SelectMacroBeforeSaving"),
                Translate("Ok"),
                DisappearingToken);
            return;
        }

        await _dialogService.ShowProgressDialogAsync(
            false,
            async (progressDialog, token) =>
            {
                if (ControllerAction != null)
                {
                    await _creationManager.UpdateControllerActionAsync(
                        ControllerAction,
                        Action.DeviceId,
                        0,
                        false,
                        ControllerButtonType.DeviceMacro,
                        ControllerAxisType.Normal,
                        ControllerAxisCharacteristic.Linear,
                        100,
                        100,
                        0,
                        ChannelOutputType.NormalMotor,
                        90,
                        0,
                        90,
                        string.Empty,
                        Action.MacroId,
                        Action.MacroChoiceValue);
                }
                else
                {
                    await _creationManager.AddOrUpdateControllerActionAsync(
                        ControllerEvent!,
                        Action.DeviceId,
                        0,
                        false,
                        ControllerButtonType.DeviceMacro,
                        ControllerAxisType.Normal,
                        ControllerAxisCharacteristic.Linear,
                        100,
                        100,
                        0,
                        ChannelOutputType.NormalMotor,
                        90,
                        0,
                        90,
                        string.Empty,
                        Action.MacroId,
                        Action.MacroChoiceValue);
                }
            },
            Translate("Saving"),
            token: DisappearingToken);

        await NavigationService.NavigateBackAsync();
    }

    private async Task SelectDeviceAsync()
    {
        var candidates = DevicesWithDeviceMacros;
        if (candidates.Count == 0)
        {
            return;
        }

        var result = await _dialogService.ShowSelectionDialogAsync(
            candidates,
            Translate("SelectDevice"),
            Translate("Cancel"),
            DisappearingToken);

        if (result.IsOk)
        {
            SelectedDevice = result.SelectedItem;
        }
    }

    private async Task SelectMacroAsync()
    {
        var macros = AvailableMacros;
        if (macros.Count == 0)
        {
            return;
        }

        var labels = macros.Select(m => Translate(m.NameKey)).ToArray();

        var result = await _dialogService.ShowSelectionDialogAsync(
            labels,
            Translate("SelectMacro"),
            Translate("Cancel"),
            DisappearingToken);

        if (result.IsOk)
        {
            var index = Array.IndexOf(labels, result.SelectedItem);
            if (index >= 0)
            {
                var macro = macros[index];
                Action.MacroId = macro.Id;
                Action.MacroChoiceValue = macro.Choices.Count > 0 ? (int)macro.Choices[0].BoxedValue : null;
                RaisePropertyChanged(nameof(SelectedMacro));
                RaisePropertyChanged(nameof(SelectedMacroDisplayName));
                RaisePropertyChanged(nameof(SelectedMacroChoiceDisplayName));
            }
        }
    }

    private async Task SelectMacroChoiceAsync()
    {
        var macro = SelectedMacro;
        if (macro is null || macro.Choices.Count == 0)
        {
            return;
        }

        var labels = macro.Choices.Select(c => Translate(c.LabelKey)).ToArray();

        var result = await _dialogService.ShowSelectionDialogAsync(
            labels,
            Translate("SelectMacroChoice"),
            Translate("Cancel"),
            DisappearingToken);

        if (result.IsOk)
        {
            var index = Array.IndexOf(labels, result.SelectedItem);
            if (index >= 0)
            {
                Action.MacroChoiceValue = (int)macro.Choices[index].BoxedValue;
                RaisePropertyChanged(nameof(SelectedMacroChoiceDisplayName));
            }
        }
    }
}
