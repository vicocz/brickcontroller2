using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class DeviceBoolSettingViewModel : DeviceSettingViewModelBase
    {
        public DeviceBoolSettingViewModel(DeviceSettingsPageViewModel parent, DeviceSetting setting, ITranslationService translationService) 
            : base(parent, setting, translationService)
        {
        }

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
