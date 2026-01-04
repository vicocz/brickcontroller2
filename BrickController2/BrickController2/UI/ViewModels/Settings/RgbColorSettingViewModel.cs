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
            Value = Color.FromRgb(value / 255f, Green, Blue);
            RaisePropertyChanged();
        }
    }
    public float Green
    {
        get => RgbValue.G * 255f;
        set
        {
            Value = Color.FromRgb(Red, value / 255f, Blue);
            RaisePropertyChanged();
        }
    }
    public float Blue
    {
        get => RgbValue.B * 255f;
        set
        {
            Value = Color.FromRgb(Red, Green, value / 255f);
            RaisePropertyChanged();
        }
    }
    private RgbColor RgbValue
    {
        get => (RgbColor)SettingValue;
    }
    private static Color ToColor(RgbColor color) => Color.FromRgb(color.R, color.G, color.B);
}
