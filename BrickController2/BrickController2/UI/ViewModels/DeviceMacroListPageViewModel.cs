using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Macros;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels;

public class DeviceMacroListPageViewModel : PageViewModelBase
{
    private readonly IDeviceManager _deviceManager;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _connectionTokenSource;
    private Task? _connectionTask;
    private bool _isDisappearing = false;

    public DeviceMacroListPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
            IDeviceManager deviceManager,
        IDialogService dialogService,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        _deviceManager = deviceManager;
        _dialogService = dialogService;

        Device = parameters.Get<Device>("device");

        Macros = [.. Device.AvailableMacros
            .Where(m => m.Scope == MacroScope.Device)
            .Select(m => new MacroItemViewModel(this, m))];

        ExecuteMacroCommand = new SafeCommand<MacroItemViewModel>(
            ExecuteMacroAsync,
            _ => Device.DeviceState == DeviceState.Connected && !_dialogService.IsDialogOpen);
    }

    public Device Device { get; }
    public IReadOnlyList<MacroItemViewModel> Macros { get; }

    public ICommand ExecuteMacroCommand { get; }

    public override async void OnAppearing()
    {
        _isDisappearing = false;
        base.OnAppearing();

        if (Device is IBluetoothDevice)
        {
            if (!await _deviceManager.IsBluetoothOnAsync())
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("TurnOnBluetoothToConnect"),
                    Translate("Ok"),
                    DisappearingToken);

                if (!_isDisappearing)
                {
                    await NavigationService.NavigateBackAsync();
                }
                return;
            }
        }

        _connectionTokenSource = new CancellationTokenSource();
        _connectionTask = ConnectAsync();
    }

    public override async void OnDisappearing()
    {
        _isDisappearing = true;
        base.OnDisappearing();

        if (_connectionTokenSource is not null && _connectionTask is not null)
        {
            _connectionTokenSource.Cancel();
            await _connectionTask;
        }

        await Device.DisconnectAsync();
    }

    private async Task ConnectAsync()
    {
        while (!(_connectionTokenSource?.IsCancellationRequested ?? false))
        {
            if (Device.DeviceState != DeviceState.Connected)
            {
                var connectionResult = DeviceConnectionResult.Ok;

                var dialogResult = await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) =>
                    {
                        using (token.Register(() => _connectionTokenSource?.Cancel()))
                        {
                            await Device.ConnectAsync(
                                false,
                                OnDeviceDisconnected,
                                [],
                                false,
                                false,
                                token);
                        }
                    },
                    Translate("ConnectingTo"),
                    Device.Name,
                    Translate("Cancel"),
                    _connectionTokenSource?.Token ?? default);

                if (dialogResult.IsCancelled)
                {
                    await Device.DisconnectAsync();

                    if (!_isDisappearing)
                    {
                        await NavigationService.NavigateBackAsync();
                    }

                    return;
                }
                else
                {
                    if (connectionResult == DeviceConnectionResult.Error)
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("FailedToConnect"),
                            Translate("Ok"),
                            DisappearingToken);

                        if (!_isDisappearing)
                        {
                            await NavigationService.NavigateBackAsync();
                        }

                        return;
                    }
                }
            }
            else
            {
                await Task.Delay(50);
            }
        }
    }

    private void OnDeviceDisconnected(Device device)
    {
    }

    private async Task ExecuteMacroAsync(MacroItemViewModel macroItem)
    {
        var descriptor = macroItem.Descriptor;
        object? choiceValue = null;

        if (descriptor.Choices.Count > 0)
        {
            var labels = descriptor.Choices.Select(c => Translate(c.LabelKey)).ToArray();

            var result = await _dialogService.ShowSelectionDialogAsync(
                labels,
                Translate("SelectMacroChoice"),
                Translate("Cancel"),
                DisappearingToken);

            if (!result.IsOk)
            {
                return;
            }

            var index = Array.IndexOf(labels, result.SelectedItem);
            if (index < 0)
            {
                return;
            }

            choiceValue = descriptor.Choices[index].BoxedValue;
        }

        try
        {
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) => await Device.ExecuteMacroAsync(new MacroInvocation(descriptor.Id, choiceValue, null), token),
                Translate("Applying"),
                token: DisappearingToken);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageBoxAsync(
                Translate("Warning"),
                Translate("ExecuteMacroFailed", ex),
                Translate("Ok"),
                DisappearingToken);
        }
    }

    public class MacroItemViewModel
    {
        private readonly DeviceMacroListPageViewModel _pageViewModel;

        public MacroItemViewModel(DeviceMacroListPageViewModel pageViewModel, MacroDescriptor descriptor)
        {
            _pageViewModel = pageViewModel;
            Descriptor = descriptor;
        }

        public MacroDescriptor Descriptor { get; }
        public string DisplayName => _pageViewModel.Translate(Descriptor.NameKey);
    }
}
