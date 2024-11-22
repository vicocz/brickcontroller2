using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Translation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels
{
    public abstract class DeviceSettingViewModelBase : NotifyPropertyChangedSource
    {
        protected readonly ITranslationService TranslationService;

        protected DeviceSettingViewModelBase(DeviceSettingsPageViewModel parent,
            DeviceSetting setting,
            ITranslationService translationService)
        {
            Setting = setting with { };
            Parent = parent;
            TranslationService = translationService;
        }

        public string DisplayName => TranslationService.Translate(Setting.Name);

        public bool HasChanged { get; protected set; }

        public DeviceSetting Setting { get; }
        public DeviceSettingsPageViewModel Parent { get; }

        protected Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items) where T : notnull
            => Parent.DialogService.ShowSelectionDialogAsync(
                items,
                DisplayName,
                TranslationService.Translate("Cancel"),
                Parent.DisappearingToken);
    }
}
