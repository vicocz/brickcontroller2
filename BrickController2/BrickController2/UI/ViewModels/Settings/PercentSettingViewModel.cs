using BrickController2.Settings;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels.Settings;

public class PercentSettingViewModel : SettingViewModelBase<double>
{
    public PercentSettingViewModel(NamedSetting setting,
        SettingsPageViewModelBase parent,
        ITranslationService translationService)
        : base(setting, parent, translationService)
    {
    }

    public double Min => Percent.MinValue;
    public double Max => Percent.MaxValue;

    public override double Value
    {
        get => ((Percent)SettingValue).Value;
        set => SettingValue = (Percent)(float)value;
    }
}
