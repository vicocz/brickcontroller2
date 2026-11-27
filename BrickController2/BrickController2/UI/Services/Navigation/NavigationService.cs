using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BrickController2.UI.DI;
using BrickController2.UI.Pages;
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

        public async Task NavigateToMoreAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase
        {
            if (_isSwitchingRoot || !State.IsNavigationEnabled || RootPage is not MainTabbedPage tabbedPage)
            {
                return;
            }

            _isSwitchingRoot = true;
            try
            {
                var moreTab = await SwitchTabAsync(tabbedPage, RootKeys.More);
                if (moreTab is not null)
                {
                    await PushMoreAsync<T>(moreTab.Navigation, parameters);
                }
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

        private async Task<NavigationPage?> SwitchTabAsync(MainTabbedPage tabbedPage, string rootKey)
        {
            var tab = tabbedPage.FindTab(rootKey);
            if (tab is null)
            {
                return null;
            }

            if (tab.Navigation.NavigationStack.Count > 1)
            {
                await tab.PopToRootAsync(false);
            }

            tabbedPage.CurrentPage = tab;
            State.ActiveRootKey = rootKey;
            return tab;
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

        private static Page? RootPage => Application.Current?.Windows.FirstOrDefault()?.Page;

        private static INavigation Navigation => RootPage switch
        {
            TabbedPage { CurrentPage: { } currentTab } => currentTab.Navigation,
            { } page => page.Navigation,
            _ => throw new InvalidOperationException("root page not available.")
        };

        private Type GetPageType<T>() where T : PageViewModelBase
        {
            var pageTypeName = typeof(T).FullName!.Replace(".ViewModels.", ".Pages.").Replace("PageViewModel", "Page");
            var pageType = Assembly.GetExecutingAssembly().GetType(pageTypeName) ??
                throw new InvalidOperationException("page type not found.");
            return pageType;
        }
    }
}
