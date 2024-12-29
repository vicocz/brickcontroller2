using BrickController2.UI.Services.Translation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BrickController2.UI.ViewModels.Settings;

public class DeviceSettingGroupViewModel : ObservableCollection<DeviceSettingViewModelBase>
{
    private readonly string _groupName;
    private readonly ITranslationService _translationService;

    public DeviceSettingGroupViewModel(string groupName,
        IEnumerable<DeviceSettingViewModelBase> settings,
        ITranslationService translationService)
        : base(settings)
    {
        _groupName = groupName;
        _translationService = translationService;
    }

    public bool HasNonDefaultValue => this.Any(x => x.HasNonDefaultValue);

    public string GroupName => _translationService.Translate(_groupName);

    internal void ResetToDefaults()
    {
        foreach (var setting in this.Where(s => s.HasNonDefaultValue))
        {
            setting.ResetToDefault();
        }
    }

    internal void OnSettingChanged() => base.OnPropertyChanged(new(nameof(HasNonDefaultValue)));
}
