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
        private readonly object _originalValue;
        protected readonly ITranslationService TranslationService;

        protected DeviceSettingViewModelBase(DeviceSettingsPageViewModel parent,
            DeviceSetting setting,
            ITranslationService translationService)
        {
            _originalValue = setting.Value;
            Setting = setting with { };
            Parent = parent;
            TranslationService = translationService;
        }

        public string DisplayName => TranslationService.Translate(Setting.Name);

        public bool HasChanged => !Setting.Value.Equals(_originalValue);

        public bool HasNonDefaultValue => !Setting.Value.Equals(Setting.DefaultValue);

        public DeviceSetting Setting { get; }
        public DeviceSettingsPageViewModel Parent { get; }

        internal abstract void ResetToDefault();

        protected Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items) where T : notnull
            => Parent.DialogService.ShowSelectionDialogAsync(
                items,
                DisplayName,
                TranslationService.Translate("Cancel"),
                Parent.DisappearingToken);
    }
}
