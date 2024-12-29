using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class DeviceEnumSettingViewModel : DeviceSettingViewModelBase
    {
        public DeviceEnumSettingViewModel(DeviceSettingsPageViewModel parentModel,
            DeviceSetting setting,
            ITranslationService translationService)
             : base(parentModel, setting, translationService)
        {
            SelectItemCommand = new SafeCommand(SelectItemAsync);
        }

        public IEnumerable<string> Items => Enum.GetNames(Setting.Type);
        public ICommand SelectItemCommand { get; }

        public string CurrentItem
        {
            get => Enum.GetName(Setting.Type, Setting.Value)!;
            set
            {
                var enumValue = Enum.Parse(Setting.Type, value);
                if (!enumValue.Equals(Setting.Value))
                {
                    Setting.Value = enumValue;
                    RaisePropertyChanged();
                    Parent.OnSettingChanged();
                }
            }
        }

        internal override void ResetToDefault()
        {
            CurrentItem = Enum.GetName(Setting.Type, Setting.DefaultValue)!;
        }

        private async Task SelectItemAsync()
        {
            var result = await ShowSelectionDialogAsync<string>(Items);
            if (result.IsOk)
            {
                CurrentItem = result.SelectedItem;
            }
        }
    }
}
