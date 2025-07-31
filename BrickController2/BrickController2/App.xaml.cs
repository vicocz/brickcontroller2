using BrickController2.UI.DI;
using BrickController2.UI.Pages;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using System;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace BrickController2
{
	public partial class App : Application
	{
        private readonly ViewModelFactory _viewModelFactory;
        private readonly PageFactory _pageFactory;
        private readonly Func<Page, NavigationPage> _navigationPageFactory;
        private readonly BackgroundService _backgroundService;

		public App(
            ViewModelFactory viewModelFactory, 
            PageFactory pageFactory, 
            Func<Page, NavigationPage> navigationPageFactory,
            BackgroundService backgroundService,
			IThemeService themeService,
			ILocalizationService localizationService)
		{
			InitializeComponent();

            _viewModelFactory = viewModelFactory;
            _pageFactory = pageFactory;
            _navigationPageFactory = navigationPageFactory;
            _backgroundService = backgroundService;

			Application.Current!.RequestedThemeChanged += (s, e) =>
			{
				themeService.CurrentTheme = e.RequestedTheme switch
				{
					AppTheme.Dark => ThemeType.Dark,
					AppTheme.Light => ThemeType.Light,
					_ => ThemeType.System
				};
				themeService.ApplyCurrentTheme();
			};

            localizationService.ApplyCurrentLanguage();
            themeService.ApplyCurrentTheme();
		}

        internal void ReloadRootPage()
        {
            // recreate the root page to apply the change
            if (Windows[0].Page is NavigationPage navigationPage &&
                navigationPage.RootPage.BindingContext is CreationListPageViewModel viewModel)
            {
                // reset view model
                navigationPage.RootPage.BindingContext = null;
                // apply new page with the existing view model
                Windows[0].Page = GetMainPage(viewModel);
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            NavigationPage navigationPage = GetMainPage();
            return new Window(navigationPage);
        }

        private NavigationPage GetMainPage(CreationListPageViewModel? viewModel = default)
        {
            var vm = viewModel ?? _viewModelFactory(typeof(CreationListPageViewModel), null);
            var page = _pageFactory(typeof(CreationListPage), vm);
            var navigationPage = _navigationPageFactory(page);
            navigationPage.BarBackgroundColor = Colors.Red;
            navigationPage.BarTextColor = Colors.White;
            return navigationPage;
        }

        protected override void OnStart()
		{
		}

		protected override void OnSleep()
		{
            _backgroundService.FireApplicationSleepEvent();
		}

		protected override void OnResume()
		{
            _backgroundService.FireApplicationResumeEvent();
		}
	}
}
