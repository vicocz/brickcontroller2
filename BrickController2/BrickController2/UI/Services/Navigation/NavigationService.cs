using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BrickController2.UI.DI;
using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly PageFactory _pageFactory;
        private readonly ViewModelFactory _viewModelFactory;

        private readonly Dictionary<Type, PageViewModelBase> _rootViewModels = [];
        private bool _isSwitchingRoot;

        public NavigationService(PageFactory pageFactory, ViewModelFactory viewModelFactory, NavigationState state)
        {
            _pageFactory = pageFactory;
            _viewModelFactory = viewModelFactory;
            State = state;
        }

        public NavigationState State { get; }

        public void RegisterRootViewModel(PageViewModelBase viewModel) => _rootViewModels[viewModel.GetType()] = viewModel;

        public T? GetRootViewModel<T>() where T : PageViewModelBase =>
            _rootViewModels.TryGetValue(typeof(T), out var vm) ? (T)vm : null;

        public async Task NavigateToRootAsync<T>(string rootKey, NavigationParameters? parameters = null) where T : PageViewModelBase
        {
            State.IsMoreMenuOpen = false;
            if (_isSwitchingRoot || !State.IsNavigationEnabled)
            {
                return;
            }

            _isSwitchingRoot = true;
            try
            {
                var navigation = Navigation;
                var oldRoot = navigation.NavigationStack.FirstOrDefault();

                if (oldRoot?.BindingContext is T)
                {
                    if (navigation.NavigationStack.Count > 1)
                    {
                        await navigation.PopToRootAsync(false);
                    }
                }
                else
                {
                    var vm = GetRootViewModel<T>() ?? _viewModelFactory(typeof(T), parameters);
                    RegisterRootViewModel(vm);
                    var page = _pageFactory(GetPageType<T>(), vm);

                    if (oldRoot is null)
                    {
                        await navigation.PushAsync(page, false);
                    }
                    else
                    {
                        navigation.InsertPageBefore(page, oldRoot);
                        await navigation.PopToRootAsync(false);
                    }
                }

                State.ActiveRootKey = rootKey;
            }
            finally
            {
                _isSwitchingRoot = false;
            }
        }

        public async Task NavigateToMoreAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase
        {
            State.IsMoreMenuOpen = false;
            if (_isSwitchingRoot || !State.IsNavigationEnabled)
            {
                return;
            }

            _isSwitchingRoot = true;
            try
            {
                var navigation = Navigation;
                var top = navigation.NavigationStack.LastOrDefault();
                if (top?.BindingContext is PageViewModelBase { RootKey: RootKeys.More } current)
                {
                    if (current is T)
                    {
                        return;
                    }

                    // replace the current More page instead of stacking another one
                    await PushMoreAsync<T>(navigation, parameters);
                    navigation.RemovePage(top);
                }
                else
                {
                    await PushMoreAsync<T>(navigation, parameters);
                }

                State.ActiveRootKey = RootKeys.More;
            }
            finally
            {
                _isSwitchingRoot = false;
            }
        }

        private Task PushMoreAsync<T>(INavigation navigation, NavigationParameters? parameters) where T : PageViewModelBase
        {
            var vm = _viewModelFactory(typeof(T), parameters);
            var page = _pageFactory(GetPageType<T>(), vm);
            return navigation.PushAsync(page, false);
        }

        public void UpdateActiveRoot()
        {
            var stack = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation.NavigationStack;
            if (stack is null)
            {
                return;
            }

            for (var i = stack.Count - 1; i >= 0; i--)
            {
                if (stack[i]?.BindingContext is PageViewModelBase { RootKey: { } key })
                {
                    State.ActiveRootKey = key;
                    return;
                }
            }
        }

        public Task NavigateToAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase
        {
            var vm = _viewModelFactory(typeof(T), parameters);
            var page = _pageFactory(GetPageType<T>(), vm);
            return Navigation.PushAsync(page);
        }

        public Task NavigateToModalAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase
        {
            var vm = _viewModelFactory(typeof(T), parameters);
            var page = _pageFactory(GetPageType<T>(), vm);
            return Navigation.PushModalAsync(page);
        }

        public Task NavigateBackAsync()
        {
            return Navigation.PopAsync();
        }

        public Task NavigateModalBackAsync()
        {
            return Navigation.PopAsync();
        }

        private static INavigation Navigation => Application.Current!.Windows[0].Page!.Navigation;

        private Type GetPageType<T>() where T : PageViewModelBase
        {
            var pageTypeName = typeof(T).FullName!.Replace(".ViewModels.", ".Pages.").Replace("PageViewModel", "Page");
            var pageType = Assembly.GetExecutingAssembly().GetType(pageTypeName) ??
                throw new InvalidOperationException("page type not found.");
            return pageType;
        }
    }
}
