using BrickController2.Extensions;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using static BrickController2.PlatformServices.GameController.GameControllers;

namespace BrickController2.UI.ViewModels
{
    public class ControllerTesterPageViewModel : PageViewModelBase
    {
        private readonly IGameControllerService _gameControllerService;
        private readonly ObservableCollection<GameControllerEventViewModel> _events = [];
        private ObservableGameControllerCollection _groups = [];

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
                _gameControllerService.CollectionChanged += GameController_CollectionChanged;
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
            _gameControllerService.CollectionChanged -= GameController_CollectionChanged;
            _gameControllerService.GameControllerEvent -= GameControllerEventHandler_Grouping!;
            _gameControllerService.GameControllerEvent -= GameControllerEventHandler!;
        }

        private void GameController_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _groups.AddRange(e.NewItems!.Cast<IGameController>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // -- workaround
                    // MAUI could not handle removal of a group
                    var removedItems = e.OldItems!.Cast<IGameController>()
                        .Select(x => x.ControllerId)
                        .ToHashSet();
                    _groups = new(_groups.Where(x => !removedItems.Contains(x.ControllerId)));
                    // notify
                    RaisePropertyChanged(nameof(ControllerEventList));
                    // -- original code
                    //_groups.RemoveAll(e.OldItems!.Cast<IGameController>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _groups.Clear();
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
                    _gameControllerService.TryGetController(args.ControllerId, out var controller);
                    group = new GameControllerGroupViewModel(args.ControllerId, controller);
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

        private class ObservableGameControllerCollection : ObservableCollection<GameControllerGroupViewModel>
        {
            public ObservableGameControllerCollection()
            {
            }

            public ObservableGameControllerCollection(IEnumerable<GameControllerGroupViewModel> collection) : base(collection)
            {
            }

            // override Add to support sorting
            public new void Add(GameControllerGroupViewModel item)
            {
                if (Items is List<GameControllerGroupViewModel> list)
                {
                    var idx = list.BinarySearch(item);
                    if (idx < 0)
                    {
                        InsertItem(~idx, item);
                        return;
                    }
                }
                // fallback
                base.Add(item);
            }

            public void AddRange(IEnumerable<IGameController> controllers)
            {
                foreach (var controller in controllers)
                {
                    Add(new(controller));
                }
            }

            public void RemoveAll(IEnumerable<IGameController> controllers)
            {
                foreach (var controller in controllers)
                {
                    Items.Remove(x => x.ControllerId == controller.ControllerId, out var _);
                }
            }
        }
    }
}
