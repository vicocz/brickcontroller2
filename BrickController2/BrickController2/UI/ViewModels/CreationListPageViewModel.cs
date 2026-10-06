using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.Permission;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using ZXing.Net.Maui;


namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IPlayLogic _playLogic;
        private readonly IDialogService _dialogService;
        private readonly IBluetoothPermission _bluetoothPermission;
        private readonly IReadWriteExternalStoragePermission _readWriteExternalStoragePermission;

        private bool _isLoaded;
        private bool _isObservingCreations;

        // Permission request fires OnDisappearing somehow (WTF???)
        private bool _isRequestingPermission = false;
        private bool _isBluetoothPermissionRequested = false;
        //private bool _isLocationPermissionRequested = false;
        private bool _isStoragePermissionRequested = false;

        public CreationListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IPlayLogic playLogic,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService,
            ICommandFactory<Creation> commandFactory,
            IBluetoothPermission bluetoothPermission,
            IReadWriteExternalStoragePermission readWriteExternalStoragePermission)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _playLogic = playLogic;
            _dialogService = dialogService;
            _bluetoothPermission = bluetoothPermission;
            _readWriteExternalStoragePermission = readWriteExternalStoragePermission;
            SharedFileStorageService = sharedFileStorageService;

            ImportCreationCommand = commandFactory.ImportItemFromFileCommand(this);
            ImportCreationFromFileCommand = commandFactory.ImportItemFromJsonFileCommand(this);
            ScanCreationCommand = new SafeCommand(ScanCreationAsync, () => BarcodeScanning.IsSupported);
            PasteCreationCommand = commandFactory.PasteItemFromClipboardCommand(this);
            OpenSettingsPageCommand = new SafeCommand(async () => await navigationService.NavigateToAsync<SettingsPageViewModel>(new NavigationParameters(("parent", this))), () => !_dialogService.IsDialogOpen);
            AddCreationCommand = new SafeCommand(async () => await AddCreationAsync());
            CreationTappedCommand = new SafeCommand<CreationListItemViewModel>(async item =>
            {
                if (IsSelectingCreations) item.IsSelected = !item.IsSelected;
                else await NavigationService.NavigateToAsync<CreationPageViewModel>(new NavigationParameters(("creation", item.Creation)));
            });
            PlayAssignedCreationsCommand = new SafeCommand(() => SetSelectionMode(true));
            CancelSelectionCommand = new SafeCommand(() => SetSelectionMode(false));
            PlaySelectedCreationsCommand = new SafeCommand(PlaySelectedCreationsAsync, () => Items.Any(i => i.IsSelected));
            DeleteCreationCommand = new SafeCommand<Creation>(async creation => await DeleteCreationAsync(creation));
            PlayCreationCommand = new SafeCommand<Creation>(PlayAsync);
            ShareCreationCommand = new SafeCommand<Creation>(async creation => await NavigationService.NavigateToAsync<CreationSharePageViewModel>(new NavigationParameters(("item", creation))));
            NavigateToDevicesCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<DeviceListPageViewModel>());
            NavigateToInputDeviceTesterCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<InputDeviceTesterPageViewModel>());
            NavigateToSequencesCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<SequenceListPageViewModel>());
            NavigateToAboutCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<AboutPageViewModel>());
        }

        public ObservableCollection<Creation> Creations => _creationManager.Creations;
        public ObservableCollection<CreationListItemViewModel> Items { get; } = new();
        public bool IsSelectingCreations { get; private set; }
        public bool IsBrowsingCreations => !IsSelectingCreations;
        public string SelectionHint => Translate("SelectCreationsToPlay") + $" ({Items.Count(i => i.IsSelected)})";
        public ICommand PlayAssignedCreationsCommand { get; }
        public ICommand PlaySelectedCreationsCommand { get; }
        public ICommand CancelSelectionCommand { get; }

        private void RefreshItems()
        {
            var selectedCreations = Items.Where(i => i.IsSelected).Select(i => i.Creation).ToHashSet();
            foreach (var item in Items) item.PropertyChanged -= SelectionChanged;
            Items.Clear();
            foreach (var creation in Creations)
            {
                var text = creation.ControllerAssignmentId == null ? Translate("AnyController") :
                    creation.ControllerAssignmentId == "none" ? Translate("NoController") : creation.ControllerAssignmentName ?? Translate("ControllerAssignment");
                var item = new CreationListItemViewModel(creation, text);
                item.IsSelected = IsSelectingCreations && item.CanSelect && selectedCreations.Contains(creation);
                item.PropertyChanged += SelectionChanged;
                Items.Add(item);
            }
            SelectionChanged(this, new System.ComponentModel.PropertyChangedEventArgs(null));
        }

        private void CreationsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshItems();

        private void SelectionChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SelectionHint));
            PlaySelectedCreationsCommand.RaiseCanExecuteChanged();
        }

        private void SetSelectionMode(bool selecting)
        {
            IsSelectingCreations = selecting;
            RaisePropertyChanged(nameof(IsSelectingCreations));
            RaisePropertyChanged(nameof(IsBrowsingCreations));
            foreach (var item in Items) item.IsSelected = selecting && item.CanSelect;
            SelectionChanged(this, new System.ComponentModel.PropertyChangedEventArgs(null));
        }

        private async Task PlaySelectedCreationsAsync()
        {
            var creations = Items.Where(i => i.IsSelected).Select(i => i.Creation).ToArray();
            if (creations.Length == 0) return;
            if (creations.Any(c => c.ControllerProfiles.Count == 0 || _playLogic.ValidateCreation(c) != CreationValidationResult.Ok) ||
                creations.SelectMany(c => c.GetDeviceIds()).GroupBy(id => id).Any(g => g.Count() > 1))
            {
                await _dialogService.ShowMessageBoxAsync(Translate("Warning"), Translate("AssignedCreationsInvalid"),
                    Translate("Ok"), DisappearingToken);
                return;
            }
            await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(
                ("creation", creations[0]), ("creations", creations)));
        }

        public ISharedFileStorageService SharedFileStorageService { get; }

        public ICommand OpenSettingsPageCommand { get; }
        public ICommand AddCreationCommand { get; }
        public ICommand CreationTappedCommand { get; }
        public ICommand DeleteCreationCommand { get; }
        public ICommand PlayCreationCommand { get; }
        public ICommand ShareCreationCommand { get; }
        public ICommand ImportCreationCommand { get; }
        public ICommand ImportCreationFromFileCommand { get; }
        public ICommand PasteCreationCommand { get; }
        public ICommand ScanCreationCommand { get; }
        public ICommand NavigateToDevicesCommand { get; }
        public ICommand NavigateToInputDeviceTesterCommand { get; }
        public ICommand NavigateToSequencesCommand { get; }
        public ICommand NavigateToAboutCommand { get; }

        public override async void OnAppearing()
        {
            if (!_isRequestingPermission)
            {
                base.OnAppearing();

                if (!_isObservingCreations)
                {
                    Creations.CollectionChanged += CreationsChanged;
                    _isObservingCreations = true;
                }

                await LoadCreationsAndDevicesAsync();
                SetSelectionMode(false);
                RefreshItems();
                await RequestPermissionsAsync();
            }
        }

        public override void OnDisappearing()
        {
            if (!_isRequestingPermission)
            {
                Creations.CollectionChanged -= CreationsChanged;
                _isObservingCreations = false;
                base.OnDisappearing();
            }
        }

        private async Task RequestPermissionsAsync()
        {
            try
            {
                var bluetoothPermissionStatus = await _bluetoothPermission.CheckStatusAsync();
                if (bluetoothPermissionStatus != PermissionStatus.Granted && !_isBluetoothPermissionRequested)
                {
                    _isRequestingPermission = true;
                    bluetoothPermissionStatus = await _bluetoothPermission.RequestAsync();
                    _isBluetoothPermissionRequested = true;
                    _isRequestingPermission = false;

                    DisappearingToken.ThrowIfCancellationRequested();
                }

                if (bluetoothPermissionStatus != PermissionStatus.Granted)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("BluetoothDevicesWillNOTBeAvailable"),
                        Translate("Ok"),
                        DisappearingToken);

                    DisappearingToken.ThrowIfCancellationRequested();
                }

                if (SharedFileStorageService.SharedStorageBaseDirectory != null)
                {
                    var storagePermissionStatus = await _readWriteExternalStoragePermission.CheckStatusAsync();
                    if (storagePermissionStatus != PermissionStatus.Granted && !_isStoragePermissionRequested)
                    {
                        _isRequestingPermission = true;
                        storagePermissionStatus = await _readWriteExternalStoragePermission.RequestAsync();
                        _isStoragePermissionRequested = true;
                        _isRequestingPermission = false;

                        DisappearingToken.ThrowIfCancellationRequested();
                    }

                    SharedFileStorageService.IsPermissionGranted = storagePermissionStatus == PermissionStatus.Granted;
                    // update command enablement
                    ImportCreationCommand.RaiseCanExecuteChanged();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ScanCreationAsync()
        {
            try
            {
                await NavigationService.NavigateToAsync<CreationScannerPageViewModel>(new NavigationParameters());
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task LoadCreationsAndDevicesAsync()
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) =>
                    {
                        await _creationManager.LoadCreationsAndSequencesAsync();
                        await _deviceManager.LoadDevicesAsync();
                        _isLoaded = true;
                    },
                    Translate("Loading"),
                    token: DisappearingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task AddCreationAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    string.Empty,
                    Translate("CreationName"),
                    Translate("Create"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (creationName) => !string.IsNullOrEmpty(creationName),
                    DisappearingToken);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("CreationNameCanNotBeEmpty"),
                            Translate("Ok"),
                            DisappearingToken);

                        return;
                    }

                    Creation? creation = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            creation = await _creationManager.AddCreationAsync(result.Result);
                            await _creationManager.AddControllerProfileAsync(creation, Translate("DefaultProfile"));
                        },
                        Translate("Creating"),
                        token: DisappearingToken);

                    await NavigationService.NavigateToAsync<CreationPageViewModel>(new NavigationParameters(("creation", creation!)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteCreationAsync(Creation creation)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteCreation")} '{creation.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    DisappearingToken))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteCreationAsync(creation),
                        Translate("Deleting"),
                        token: DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PlayAsync(Creation creation)
        {
            try
            {
                var validationResult = _playLogic.ValidateCreation(creation);

                string warning = string.Empty;
                switch (validationResult)
                {
                    case CreationValidationResult.MissingControllerAction:
                        warning = Translate("NoControllerActions");
                        break;

                    case CreationValidationResult.MissingDevice:
                        warning = Translate("MissingDevices");
                        break;

                    case CreationValidationResult.MissingSequence:
                        warning = Translate("MissingSequence");
                        break;

                    case CreationValidationResult.MissingMacro:
                        warning = Translate("MissingMacro");
                        break;
                }

                if (validationResult == CreationValidationResult.Ok)
                {
                    await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(("creation", creation)));
                }
                else
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("Play") + $" '{creation.Name}': {warning}",
                        Translate("Ok"),
                        DisappearingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
