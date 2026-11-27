using BrickController2.UI.DI;
using BrickController2.UI.Pages;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace BrickController2
{
	public partial class App : Application
	{
        private readonly ViewModelFactory _viewModelFactory;
        private readonly PageFactory _pageFactory;
        private readonly Func<Page, NavigationPage> _navigationPageFactory;
        private readonly BackgroundService _backgroundService;
		private readonly INavigationService _navigationService;
		private readonly ITranslationService _translationService;

		public App(
			ViewModelFactory viewModelFactory, 
			PageFactory pageFactory, 
			Func<Page, NavigationPage> navigationPageFactory,
			BackgroundService backgroundService,
			INavigationService navigationService,
			ITranslationService translationService,
			IThemeService themeService,
			ILocalizationService localizationService)
		{
			InitializeComponent();

            _viewModelFactory = viewModelFactory;
            _pageFactory = pageFactory;
            _navigationPageFactory = navigationPageFactory;
            _backgroundService = backgroundService;
            _navigationService = navigationService;
            _translationService = translationService;

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
            if (Windows[0].Page is MainTabbedPage tabbedPage)
            {
                foreach (var tab in tabbedPage.Children.OfType<NavigationPage>())
                {
                    tab.RootPage.BindingContext = null;
                }
                Windows[0].Page = GetTabbedMainPage();
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(GetTabbedMainPage());
        }

        private MainTabbedPage GetTabbedMainPage()
        {
            _navigationService.State.ActiveRootKey = RootKeys.Creations;

            return new MainTabbedPage(_navigationService.State,
                (CreateTab<CreationListPageViewModel, CreationListPage>(), Translate("Creations"), "category"),
                (CreateTab<DeviceListPageViewModel, DeviceListPage>(), Translate("Devices"), "hub"),
                (CreateTab<SequenceListPageViewModel, SequenceListPage>(), Translate("Sequences"), "format_list_bulleted"),
                (CreateTab<MorePageViewModel, MorePage>(), Translate("More"), "more_horiz"));
        }

        private NavigationPage CreateTab<TViewModel, TPage>() where TViewModel : PageViewModelBase
        {
            var vm = _navigationService.GetRootViewModel<TViewModel>() ?? _viewModelFactory(typeof(TViewModel), null);
            _navigationService.RegisterRootViewModel(vm);
            var rootPage = _pageFactory(typeof(TPage), vm);
            if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.WinUI)
            {
                // tabs are rendered inside the toolbar on Windows, so the root title is redundant
                rootPage.Title = string.Empty;
            }
            var navigationPage = _navigationPageFactory(rootPage);
            navigationPage.BarBackgroundColor = Colors.Red;
            navigationPage.BarTextColor = Colors.White;
            return navigationPage;
        }

        private string Translate(string key) => _translationService.Translate(key);

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
