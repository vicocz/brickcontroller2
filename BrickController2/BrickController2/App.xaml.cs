using System;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using BrickController2.UI.DI;
using BrickController2.UI.ViewModels;
using BrickController2.UI.Pages;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Theme;

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
			IThemeService themeService)
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
			themeService.ApplyCurrentTheme();
		}

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var vm = _viewModelFactory(typeof(CreationListPageViewModel), null);
            var page = _pageFactory(typeof(CreationListPage), vm);
            var navigationPage = _navigationPageFactory(page);
            navigationPage.BarBackgroundColor = Colors.Red;
            navigationPage.BarTextColor = Colors.White;

            return new Window(navigationPage);
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
