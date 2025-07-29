using System;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using BrickController2.UI.DI;
using BrickController2.UI.Pages;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.ViewModels;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
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
			localizationService.LanguageChanged += (s, e) =>
			{
                // enforce language change on the main page
                Windows[0].Page = GetMainPage();
            };

            localizationService.ApplyCurrentLanguage();
            themeService.ApplyCurrentTheme();
		}

        protected override Window CreateWindow(IActivationState? activationState)
        {
            NavigationPage navigationPage = GetMainPage();
            return new Window(navigationPage);
        }

        private NavigationPage GetMainPage()
        {
            var vm = _viewModelFactory(typeof(CreationListPageViewModel), null);
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
