using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.Extensions;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using ZXing.QrCode.Internal;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class StaticDeviceListPageViewModel : PageViewModelBase
    {
        public class StaticDeviceEntry
        {
            public IStaticDeviceFactoryData StaticDeviceFactoryData { get; }
            public Device? ExistingDevice { get; }
            public bool Selected { get; set; }

            public StaticDeviceEntry(IStaticDeviceFactoryData staticDeviceFactoryData, Device? instace)
            {
                StaticDeviceFactoryData = staticDeviceFactoryData;
                ExistingDevice = instace;
                Selected = instace != null;
            }
        }
        public class StaticDeviceGroup : List<StaticDeviceEntry>
        {
            public DeviceType DeviceType { get; }

            public StaticDeviceGroup(DeviceType deviceType, List<StaticDeviceEntry> staticDeviceEntry) : base(staticDeviceEntry)
            {
                DeviceType = deviceType;
            }
        }

        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public StaticDeviceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IStaticDeviceManager staticDeviceManager,
            IDialogService dialogService) 
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            var groups = staticDeviceManager.FactoryDataList
                .GroupBy(o => o.DeviceType, x => new StaticDeviceEntry(x, GetDeviceInstance(x)));

            GroupedFactoryDatas.AddRange(groups.Select(item => new StaticDeviceGroup(item.Key, item.ToList())));

            ApplyChangesCommand = new SafeCommand(async () => await ApplyChangesAsync());
        }

        public List<StaticDeviceGroup> GroupedFactoryDatas { get; } = new List<StaticDeviceGroup>();

        public ICommand ApplyChangesCommand { get; }


        private async Task ApplyChangesAsync()
        {
            // get all entries to create (=> entry.Selected && entry.ExistingDevice == null)
            IStaticDeviceFactoryData[] devicesToCreate = GroupedFactoryDatas
                .SelectMany(group => group.FindAll(entry => entry.Selected && entry.ExistingDevice == null)
                    .Select(entry => entry.StaticDeviceFactoryData))
                .ToArray();

            // get all entries to delete (=> !entry.Selected && entry.ExistingDevice != null)
            Device[] devicesToDelete = GroupedFactoryDatas
                .SelectMany(group => group.FindAll(entry => !entry.Selected && entry.ExistingDevice != null)
                    .Select(entry => entry.ExistingDevice!))
                .ToArray();

            if (devicesToCreate.Length > 0 ||
                devicesToDelete.Length > 0)
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => 
                    {
                        if (devicesToCreate.Length > 0)
                        {
                            await _deviceManager.CreateDevicesAsync(devicesToCreate);
                        }

                        if (devicesToDelete.Length > 0)
                        {
                            await _deviceManager.DeleteDevicesAsync(devicesToDelete);
                        }
                    },
                    Translate("Applying"));
            }

            await NavigationService.NavigateBackAsync();
        }

        /// <summary>
        /// get matching device from DeviceManager or null
        /// </summary>
        /// <param name="staticDeviceFactoryData"></param>
        /// <returns>existing device or null</returns>
        private Device? GetDeviceInstance(IStaticDeviceFactoryData staticDeviceFactoryData)
        {
            return _deviceManager.Devices.FirstOrDefault(d => d.DeviceType == staticDeviceFactoryData.DeviceType && d.Address == staticDeviceFactoryData.Address);
        }
    }
}
