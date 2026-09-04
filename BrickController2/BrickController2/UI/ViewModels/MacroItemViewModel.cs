using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Macros;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Translation;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels;

public class MacroItemViewModel
{
    private readonly Device _device;
    private readonly MacroDescriptor _descriptor;
    private readonly ITranslationService _translationService;
    private readonly IDialogService _dialogService;

    public MacroItemViewModel(Device device, MacroDescriptor descriptor, ITranslationService translationService, IDialogService dialogService)
    {
        _device = device;
        _descriptor = descriptor;
        _translationService = translationService;
        _dialogService = dialogService;

        ExecuteMacroCommand = new SafeCommand(ExecuteMacroAsync, () => _device.DeviceState == DeviceState.Connected && !_dialogService.IsDialogOpen);
    }

    public ICommand ExecuteMacroCommand { get; }

    public int Idx => Math.Abs(_descriptor.Id.GetHashCode());
    public string DisplayName => _translationService.Translate(_descriptor.NameKey);
    public string Scope => _translationService.Translate(_descriptor.Scope.ToString());

    private string Translate(string key) => _translationService.Translate(key);

    private async Task ExecuteMacroAsync()
    {
        object? choiceValue = null;

        if (_descriptor.Choices.Count > 0)
        {
            var labels = _descriptor.Choices.Select(c => Translate(c.LabelKey)).ToArray();

            var result = await _dialogService.ShowSelectionDialogAsync(
                labels,
                Translate("SelectMacroChoice"),
                Translate("Cancel"),
                default);

            if (!result.IsOk)
            {
                return;
            }

            var index = Array.IndexOf(labels, result.SelectedItem);
            if (index < 0)
            {
                return;
            }

            choiceValue = _descriptor.Choices[index].BoxedValue;
        }

        try
        {
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) => await _device.ExecuteMacroAsync(new MacroInvocation(_descriptor.Id, choiceValue, null), token),
                Translate("Applying"),
                token: default);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageBoxAsync(
                Translate("Warning"),
                Translate("ExecuteMacroFailed") + ": " + ex.Message,
                Translate("Ok"),
                default);
        }
    }
}
