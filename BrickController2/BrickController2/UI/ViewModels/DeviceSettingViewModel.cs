using BrickController2.Helpers;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels;

public class DeviceSettingViewModel : NotifyPropertyChangedSource
{
    private readonly ITranslationService _translationService;
    private readonly string _name;
    private object _value;

    public DeviceSettingViewModel(KeyValuePair<string, object> setting, ITranslationService translationService)
    {
        _translationService = translationService;
        _name = setting.Key;
        _value = setting.Value;
    }

    public string Name => _name;
    public string DisplayName => _translationService.Translate(_name);

    public bool IsBoolType => _value?.GetType() == typeof(bool);

    public bool HasChanged { get; private set; }

    public object Value
    {
        get { return _value; }
        set
        {
            HasChanged |= _value != value;

            _value = value;
            RaisePropertyChanged();
        }
    }
    public KeyValuePair<string, object> KeyValuePair => new(_name, _value);
}
