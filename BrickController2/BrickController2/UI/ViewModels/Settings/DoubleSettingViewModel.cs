using BrickController2.Settings;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels.Settings;

public class DoubleSettingViewModel : SettingViewModelBase<double>
{
    public DoubleSettingViewModel(NamedSetting setting,
        SettingsPageViewModelBase parent,
        ITranslationService translationService)
        : base(setting, parent, translationService)
    {
    }

    public override double Value
    {
        get => (double)SettingValue;
        set => SettingValue = value!;
    }
}
