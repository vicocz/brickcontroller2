using BrickController2.UI.Services.Help;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels
{
    public class HelpPageViewModel : PageViewModelBase
    {
        private readonly IHelpService _helpService;
        private readonly HelpTopic _topic;

        private bool _isLoaded;
        private string _title;
        private WebViewSource? _source;

        public HelpPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IHelpService helpService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _helpService = helpService;
            _topic = parameters.Get<HelpTopic>("topic");
            _title = Translate("Help");
        }

        public string Title
        {
            get => _title;
            private set { _title = value; RaisePropertyChanged(); }
        }

        public WebViewSource? Source
        {
            get => _source;
            private set { _source = value; RaisePropertyChanged(); }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            if (!_isLoaded)
            {
                _isLoaded = true;
                _ = LoadAsync();
            }
        }

        /// <summary>
        /// Handles navigation request of the web view. Returns true when handled (the web view navigation should be cancelled).
        /// </summary>
        public bool TryHandleNavigation(string url)
        {
            if (url.StartsWith(HelpService.LinkScheme, StringComparison.OrdinalIgnoreCase))
            {
                var topic = new HelpTopic(url[HelpService.LinkScheme.Length..].Trim('/'));
                if (_helpService.HasHelp(topic))
                {
                    _ = NavigationService.NavigateToAsync<HelpPageViewModel>(new NavigationParameters(("topic", topic)));
                }

                return true;
            }

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                _ = OpenExternalAsync(url);
                return true;
            }

            return false;
        }

        private async Task LoadAsync()
        {
            var markdown = await _helpService.GetHelpMarkdownAsync(_topic);
            if (markdown is null)
            {
                return;
            }

            Title = HelpHtmlRenderer.GetTitle(markdown) ?? Translate("Help");
            Source = new HtmlWebViewSource { Html = HelpHtmlRenderer.Render(markdown, _topic, _helpService) };
        }

        private static async Task OpenExternalAsync(string url)
        {
            try
            {
                await Launcher.Default.OpenAsync(url);
            }
            catch
            {
                // ignore failure to open an external link
            }
        }
    }
}
