using BrickController2.Settings;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.Graphics;

namespace BrickController2.UI.ViewModels.Settings;

public class RgbColorSettingViewModel : SettingViewModelBase<Color>
{
    public RgbColorSettingViewModel(NamedSetting setting,
        SettingsPageViewModelBase parent,
        ITranslationService translationService)
        : base(setting, parent, translationService)
    {
    }

    public override Color Value
    {
        get => ToColor(RgbValue);
        set => SettingValue = new RgbColor(value!.Red, value!.Green, value!.Blue);
    }

    public float Red
    {
        get => RgbValue.R * 255f;
        set
        {
            Value = Color.FromRgb(value / 255f, RgbValue.G, RgbValue.B);
            RaisePropertyChanged();
        }
    }
    public float Green
    {
        get => RgbValue.G * 255f;
        set
        {
            Value = Color.FromRgb(RgbValue.R, value / 255f, RgbValue.B);
            RaisePropertyChanged();
        }
    }
    public float Blue
    {
        get => RgbValue.B * 255f;
        set
        {
            Value = Color.FromRgb(RgbValue.R, RgbValue.G, value / 255f);
            RaisePropertyChanged();
        }
    }

    protected override void OnValueChanged(object value)
    {
        base.OnValueChanged(value);
        // notify UI about RGB component changes
        RaisePropertyChanged(nameof(Red));
        RaisePropertyChanged(nameof(Green));
        RaisePropertyChanged(nameof(Blue));
    }

    private RgbColor RgbValue => (RgbColor)SettingValue;

    private static Color ToColor(RgbColor color) => Color.FromRgb(color.R, color.G, color.B);
}
