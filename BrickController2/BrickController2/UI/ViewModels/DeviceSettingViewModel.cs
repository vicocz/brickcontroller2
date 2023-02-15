using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Services.Translation;
using System;

namespace BrickController2.UI.ViewModels
{
    public class DeviceSettingViewModel : NotifyPropertyChangedSource
    {
        private readonly ITranslationService _translationService;

        private object _value;

        public DeviceSettingViewModel(
            Device device,
            DeviceSetting setting,
            ITranslationService translationService) 
        {
            _translationService = translationService;

            ValueType = setting.Type;
            SettingName = setting.SettingName;

            _value = device.CurrentSettings.TryGetValue(setting.SettingName, out var value) ? value : setting.DefaultValue;
        }

        public string SettingName { get; }

        public string DisplayName => _translationService.Translate(SettingName);

        public Type ValueType { get; }

        public bool IsBoolType => ValueType == typeof(bool);

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
    }
}
