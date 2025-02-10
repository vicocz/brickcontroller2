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
        private readonly ObservableCollection<GameControllerGroupViewModel> _groups = [];
        private readonly ObservableCollection<GameControllerEventViewModel> _events = [];

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
            _gameControllerService.GameControllerEvent += GameControllerEventHandler!;
        }

        public override void OnDisappearing()
        {
            _gameControllerService.GameControllerEvent -= GameControllerEventHandler!;
        }

        private void GameControllerEventHandler(object sender, GameControllerEventArgs args)
        {
            foreach (var controllerEvent in args.ControllerEvents)
            {
                // special handling for groups
                if (IsGrouped)
                {
                    var group = _groups.FirstOrDefault(x => x.ControllerId == args.ControllerId);
                    if (group is null)
                    {
                        _gameControllerService.TryGetController(args.ControllerId, out var controller);
                        group = new GameControllerGroupViewModel(args.ControllerId, controller);
                        _groups.Add(group);
                    }
                    ProcessEvent(group, controllerEvent);
                }
                else
                {
                    ProcessEvent(_events, controllerEvent);
                }
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
