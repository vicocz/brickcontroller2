using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class DeviceSettingViewModel : NotifyPropertyChangedSource
    {
        private readonly ITranslationService _translationService;

        public DeviceSettingViewModel(DeviceSetting setting, ITranslationService translationService)
        {
            Setting = setting with { };
            _translationService = translationService;
        }

        public string DisplayName => _translationService.Translate(Setting.Name);

        public bool IsBoolType => Setting.Type == typeof(bool);

        public bool HasChanged { get; private set; }

        public DeviceSetting Setting { get; }

        public object Value
        {
            get { return Setting.Value; }
            set
            {
                HasChanged |= Setting.Value != value;

                Setting.Value = value;
                RaisePropertyChanged();
            }
        }
    }
}
