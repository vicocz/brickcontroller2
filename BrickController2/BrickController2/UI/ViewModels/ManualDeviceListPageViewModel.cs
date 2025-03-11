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
    public class ManualDeviceListPageViewModel : PageViewModelBase
    {
        public class DeviceEntry
        {
            public IDeviceFactoryData DeviceFactoryData { get; }
            public Device? ExistingDevice { get; }
            public bool Selected { get; set; }

            public DeviceEntry(IDeviceFactoryData deviceFactoryData, Device? instace)
            {
                DeviceFactoryData = deviceFactoryData;
                ExistingDevice = instace;
                Selected = instace != null;
            }
        }
        public class DeviceGroup : List<DeviceEntry>
        {
            public DeviceType DeviceType { get; }

            public DeviceGroup(DeviceType deviceType, List<DeviceEntry> deviceEntry) : base(deviceEntry)
            {
                DeviceType = deviceType;
            }
        }

        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public ManualDeviceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IManualDeviceManager manualDeviceManager,
            IDialogService dialogService) 
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            var groups = manualDeviceManager.FactoryDataList
                .GroupBy(o => o.DeviceType, x => new DeviceEntry(x, GetDeviceInstance(x)));

            GroupedFactoryDatas.AddRange(groups.Select(item => new DeviceGroup(item.Key, item.ToList())));

            ApplyChangesCommand = new SafeCommand(async () => await ApplyChangesAsync());
        }

        public List<DeviceGroup> GroupedFactoryDatas { get; } = new List<DeviceGroup>();

        public ICommand ApplyChangesCommand { get; }


        private async Task ApplyChangesAsync()
        {
            // get all entries to create (=> entry.Selected && entry.ExistingDevice == null)
            IDeviceFactoryData[] devicesToCreate = GroupedFactoryDatas
                .SelectMany(group => group.FindAll(entry => entry.Selected && entry.ExistingDevice == null)
                    .Select(entry => entry.DeviceFactoryData))
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
        /// <param name="deviceFactoryData"></param>
        /// <returns>existing device or null</returns>
        private Device? GetDeviceInstance(IDeviceFactoryData deviceFactoryData)
        {
            return _deviceManager.Devices.FirstOrDefault(d => d.DeviceType == deviceFactoryData.DeviceType && d.Address == deviceFactoryData.Address);
        }
    }
}
