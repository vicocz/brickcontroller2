using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.UI.ViewModels
{
    public class DeviceSettingViewModel : NotifyPropertyChangedSource
    {
        private readonly ITranslationService _translationService;

        public DeviceSettingViewModel(DeviceSetting setting, ITranslationService translationService)
        {
            Setting = setting with { };
            _translationService = translationService;
        }

        public string DisplayName => _translationService.Translate(Setting.Name);

        public bool IsBoolType => Setting.Type == typeof(bool);
        public bool IsEnumType => Setting.Type.IsEnum;

        public bool HasChanged { get; private set; }

        public DeviceSetting Setting { get; }

        public IEnumerable<object> Items
        {
            get
            {
                if (IsEnumType)
                {
                    return Enum.GetValues(Setting.Type).Cast<Enum>();
                }
                return [];
            }
        }

        public object Value
        {
            get { return Setting.Value; }
            set
            {
                HasChanged |= Setting.Value != value;

                Setting.Value = value;
                RaisePropertyChanged();
            }
        }
    }
}
