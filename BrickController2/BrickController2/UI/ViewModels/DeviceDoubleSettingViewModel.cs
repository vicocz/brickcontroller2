using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class DeviceDoubleSettingViewModel : DeviceSettingViewModelBase
    {
        public DeviceDoubleSettingViewModel(DeviceSettingsPageViewModel parentModel,
            DeviceSetting setting,
            ITranslationService translationService)
             : base(parentModel, setting, translationService)
        {
        }

        public object MinValue => 35.0;
        public object MaxValue => 3500.0;
        public object Step => 35.0;

        public object Value
        {
            get => Setting.Value;
            set
            {
                if (Setting.Value != value)
                {
                    HasChanged |= true;
                    Setting.Value = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
