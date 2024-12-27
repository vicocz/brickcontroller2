using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class DeviceFloatSettingViewModel : DeviceSettingViewModelBase
    {
        public DeviceFloatSettingViewModel(DeviceSettingsPageViewModel parentModel,
            DeviceSetting setting,
            ITranslationService translationService)
             : base(parentModel, setting, translationService)
        {
        }

        public object MinValue => 35f;
        public object MaxValue => 3500f;
        public object Step => 35f;

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
