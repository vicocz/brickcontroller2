using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.UI.ViewModels;

public class InputDeviceTesterPageViewModel : PageViewModelBase
{
    private readonly IInputDeviceEventService _inputDeviceEventService;
    private ObservableCollection<InputDeviceGroupViewModel> _inputDeviceEventList = [];

    public InputDeviceTesterPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IInputDeviceEventService inputDeviceEventService)
        : base(navigationService, translationService)
    {
        _inputDeviceEventService = inputDeviceEventService;
    }

    public IEnumerable<INotifyPropertyChanged> InputDeviceEventList => _inputDeviceEventList;

    public override void OnAppearing()
    {
        _inputDeviceEventService.InputDevicesChangedEvent += InputDevicesChangedEventHandler;
        _inputDeviceEventService.InputDeviceEvent += InputDeviceEventHandler!;
    }

    public override void OnDisappearing()
    {
        // unregister all
        _inputDeviceEventService.InputDeviceEvent -= InputDeviceEventHandler!;
        _inputDeviceEventService.InputDevicesChangedEvent -= InputDevicesChangedEventHandler;
    }

    private void InputDevicesChangedEventHandler(object? sender, InputDeviceChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyInputDevicesChangedAction.Connected:
                // recreate collection due to MAUI could not handle adding of them
                _inputDeviceEventList = new(_inputDeviceEventList.Concat(e.Items.Select(x => new InputDeviceGroupViewModel(x)))
                    .OrderBy(x => x.InputDeviceNumber));
                // notify
                RaisePropertyChanged(nameof(InputDeviceEventList));
                break;
            case NotifyInputDevicesChangedAction.Disconnected:
                // MAUI could not handle removal of a group
                var removedItems = e.Items.Select(x => x.InputDeviceId).ToHashSet();
                _inputDeviceEventList = new(_inputDeviceEventList.Where(x => !removedItems.Contains(x.InputDeviceId)));
                // notify
                RaisePropertyChanged(nameof(InputDeviceEventList));
                break;
        }
    }

    private void InputDeviceEventHandler(object sender, InputDeviceEventArgs args)
    {
        foreach (var controllerEvent in args.InputDeviceEvents)
        {
            var group = _inputDeviceEventList.FirstOrDefault(x => x.InputDeviceId == args.InputDeviceId);
            if (group is null)
            {
                // create proxy model
                group = new InputDeviceGroupViewModel(args.InputDeviceId, default);
                _inputDeviceEventList.Add(group);
            }
            ProcessEvent(group, controllerEvent);
        }
    }

    private static void ProcessEvent(ICollection<InputDeviceEventViewModel> events,
        KeyValuePair<(InputDeviceEventType EventType, string EventCode), float> inputDeviceEvent)
    {
        var inputDeviceEventViewModel = events.FirstOrDefault(ce => ce.EventType == inputDeviceEvent.Key.EventType && ce.EventCode == inputDeviceEvent.Key.EventCode);
        if (AXIS_DELTA_VALUE < Math.Abs(inputDeviceEvent.Value))
        {
            if (inputDeviceEventViewModel != null)
            {
                inputDeviceEventViewModel.Value = inputDeviceEvent.Value;
            }
            else
            {
                events.Add(new InputDeviceEventViewModel(inputDeviceEvent.Key.EventType, inputDeviceEvent.Key.EventCode, inputDeviceEvent.Value));
            }
        }
        else
        {
            if (inputDeviceEventViewModel != null)
            {
                events.Remove(inputDeviceEventViewModel);
            }
        }
    }
}
