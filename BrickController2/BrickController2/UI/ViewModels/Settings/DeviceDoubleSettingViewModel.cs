using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels.Settings;

public class DeviceDoubleSettingViewModel : DeviceSettingViewModelBase
{
    public DeviceDoubleSettingViewModel(DeviceSettingsPageViewModel parentModel,
        DeviceSetting setting,
        ITranslationService translationService)
         : base(parentModel, setting, translationService)
    {
    }

    public object Value
    {
        get => Setting.Value;
        set
        {
            if (Setting.Value != value)
            {
                Setting.Value = value;
                RaisePropertyChanged();
                Parent.OnSettingChanged();
            }
        }
    }

    internal override void ResetToDefault()
    {
        Value = Setting.DefaultValue;
    }
}
