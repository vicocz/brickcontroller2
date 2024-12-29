using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels;

public class DeviceSettingsPageViewModel : PageViewModelBase
{
    public DeviceSettingsPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        NavigationParameters parameters) : base(navigationService, translationService)
    {
        Device = parameters.Get<Device>("device");
        Settings = new ObservableCollection<DeviceSettingViewModelBase>(Device.CurrentSettings.Select(ToViewModel));
        DialogService = dialogService;

        SaveSettingsCommand = new SafeCommand(ApplyChanges, () => Settings.Any(x => x.HasChanged));
        ResetToDefaultsCommand = new SafeCommand(ResetToDefaults, () => Settings.Any(x => x.HasNonDefaultValue));
    }

    public ICommand SaveSettingsCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }

    public Device Device { get; }
    public IDialogService DialogService { get; }

    public ObservableCollection<DeviceSettingViewModelBase> Settings { get; }

    internal void OnSettingChanged()
    {
        SaveSettingsCommand.RaiseCanExecuteChanged();
        ResetToDefaultsCommand.RaiseCanExecuteChanged();
    }

    private DeviceSettingViewModelBase ToViewModel(DeviceSetting setting)
    {
        if (setting.IsBoolType)
        {
            return new DeviceBoolSettingViewModel(this, setting, TranslationService);
        }
        if (setting.IsEnumType)
        {
            return new DeviceEnumSettingViewModel(this, setting, TranslationService);
        }
        throw new InvalidOperationException($"The specified type {setting.Type} is not supported.");
    }

    private async Task ApplyChanges()
    {
        try
        {
            await DialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    var changedSettings = Settings
                        .Where(s => s.HasChanged)
                        .Select(s => s.Setting)
                        .ToArray();

                    await Device.UpdateDeviceSettingsAsync(changedSettings);
                },
                Translate("Saving"),
                token: DisappearingToken);

            await NavigationService.NavigateBackAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ResetToDefaults()
    {
        foreach (var setting in Settings.Where(s => s.HasNonDefaultValue))
        {
            setting.ResetToDefault();
        }
    }
}
