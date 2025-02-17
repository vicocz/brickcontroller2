using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using static BrickController2.PlatformServices.GameController.GameControllers;

namespace BrickController2.UI.ViewModels
{
    public class ControllerTesterPageViewModel : PageViewModelBase
    {
        private readonly IGameControllerService _gameControllerService;
        private readonly ObservableCollection<GameControllerEventViewModel> _events = [];
        private ObservableCollection<GameControllerGroupViewModel> _groups = [];

        public ControllerTesterPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IGameControllerService gameControllerService)
            : base(navigationService, translationService)
        {
            _gameControllerService = gameControllerService;
        }

        public bool IsGrouped => _gameControllerService.IsControllerIdSupported;
        public IEnumerable<INotifyPropertyChanged> ControllerEventList => IsGrouped ? _groups : _events;

        public override void OnAppearing()
        {
            if (IsGrouped)
            {
                _gameControllerService.GameControllersChangedEvent += GameControllersChangedEventHandler;
                _gameControllerService.GameControllerEvent += GameControllerEventHandler_Grouping!;
            }
            else
            {
                _gameControllerService.GameControllerEvent += GameControllerEventHandler!;
            }
        }

        public override void OnDisappearing()
        {
            // unregister all
            _gameControllerService.GameControllerEvent -= GameControllerEventHandler_Grouping!;
            _gameControllerService.GameControllerEvent -= GameControllerEventHandler!;
            _gameControllerService.GameControllersChangedEvent -= GameControllersChangedEventHandler;
        }

        private void GameControllersChangedEventHandler(object? sender, GameControllersChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyGameControllersChangedAction.Connected:
                    // recreate collection due to MAUI could not handle adding of them
                    _groups = new(_groups.Concat(e.Items.Select(x => new GameControllerGroupViewModel(x)))
                        .OrderBy(x => x.ControllerNumber));
                    // notify
                    RaisePropertyChanged(nameof(ControllerEventList));
                    break;
                case NotifyGameControllersChangedAction.Disconnected:
                    // MAUI could not handle removal of a group
                    var removedItems = e.Items.Select(x => x.ControllerId).ToHashSet();
                    _groups = new(_groups.Where(x => !removedItems.Contains(x.ControllerId)));
                    // notify
                    RaisePropertyChanged(nameof(ControllerEventList));
                    break;
            }
        }

        private void GameControllerEventHandler_Grouping(object sender, GameControllerEventArgs args)
        {
            foreach (var controllerEvent in args.ControllerEvents)
            {
                var group = _groups.FirstOrDefault(x => x.ControllerId == args.ControllerId);
                if (group is null)
                {
                    // create proxy model
                    group = new GameControllerGroupViewModel(args.ControllerId, default);
                    _groups.Add(group);
                }
                ProcessEvent(group, controllerEvent);
            }
        }

        private void GameControllerEventHandler(object sender, GameControllerEventArgs args)
        {
            foreach (var controllerEvent in args.ControllerEvents)
            {
                ProcessEvent(_events, controllerEvent);
            }
        }

        private static void ProcessEvent(ICollection<GameControllerEventViewModel> events,
            KeyValuePair<(GameControllerEventType EventType, string EventCode), float> controllerEvent)
        {
            var controllerEventViewModel = events.FirstOrDefault(ce => ce.EventType == controllerEvent.Key.EventType && ce.EventCode == controllerEvent.Key.EventCode);
            if (AXIS_DELTA_VALUE < Math.Abs(controllerEvent.Value))
            {
                if (controllerEventViewModel != null)
                {
                    controllerEventViewModel.Value = controllerEvent.Value;
                }
                else
                {
                    events.Add(new GameControllerEventViewModel(controllerEvent.Key.EventType, controllerEvent.Key.EventCode, controllerEvent.Value));
                }
            }
            else
            {
                if (controllerEventViewModel != null)
                {
                    events.Remove(controllerEventViewModel);
                }
            }
        }
    }
}
