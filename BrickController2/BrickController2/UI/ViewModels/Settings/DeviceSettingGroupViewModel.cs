using BrickController2.UI.Services.Translation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    public string GroupName => _translationService.Translate(_groupName);
}
