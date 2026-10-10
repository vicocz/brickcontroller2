using BrickController2.UI.Commands;
using BrickController2.UI.Services.Help;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels;

public class HelpPageViewModel : PageViewModelBase
{
    private readonly IHelpService _helpService;
    private readonly IThemeService? _themeService;
    private readonly List<HelpTopic> _history = [];

    private int _index;
    private int _loadVersion;
    private bool _isLoaded;

    public HelpPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IHelpService helpService,
        NavigationParameters parameters,
        IThemeService? themeService = null)
        : base(navigationService, translationService)
    {
        _themeService = themeService;
        _helpService = helpService;
        _history.Add(parameters.Get<HelpTopic>("topic"));

        Title = Translate("Help");
        GoBackCommand = new SafeCommand(GoBackAsync, () => _index > 0);
        GoForwardCommand = new SafeCommand(GoForwardAsync, () => _index < _history.Count - 1);
    }

    private bool IsDarkTheme() => _themeService?.CurrentTheme switch
    {
        ThemeType.Dark => true,
        ThemeType.Light => false,
        _ => Application.Current?.RequestedTheme == AppTheme.Dark
    };

    /// <summary>
    /// Goes to the previous topic. Not available on the first topic (help is not left by this command).
    /// </summary>
    public ICommand GoBackCommand { get; }

    public ICommand GoForwardCommand { get; }

    public string Title
    {
        get;
        private set { field = value; RaisePropertyChanged(); }
    }

    public WebViewSource? Source
    {
        get;
        private set { field = value; RaisePropertyChanged(); }
    }

    private HelpTopic CurrentTopic => _history[_index];

    public override void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
        {
            _isLoaded = true;
            _ = LoadTopicAsync(CurrentTopic);
        }
    }

    /// <summary>
    /// Handles navigation request of the web view. Returns true when handled (the web view navigation should be cancelled).
    /// </summary>
    public bool TryHandleNavigation(string url)
    {
        if (url.StartsWith(HelpService.LinkScheme, StringComparison.OrdinalIgnoreCase))
        {
            OpenTopic(new HelpTopic(url[HelpService.LinkScheme.Length..].Trim('/')));
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

    private void OpenTopic(HelpTopic topic)
    {
        if (topic == CurrentTopic || !_helpService.HasHelp(topic))
        {
            return;
        }

        // discard forward history
        _history.RemoveRange(_index + 1, _history.Count - _index - 1);
        _history.Add(topic);
        _index++;

        GoBackCommand.RaiseCanExecuteChanged();
        GoForwardCommand.RaiseCanExecuteChanged();
        _ = LoadTopicAsync(topic);
    }

    private async Task GoBackAsync()
    {
        if (_index == 0)
        {
            return;
        }

        _index--;
        GoBackCommand.RaiseCanExecuteChanged();
        GoForwardCommand.RaiseCanExecuteChanged();
        await LoadTopicAsync(CurrentTopic);
    }

    private async Task GoForwardAsync()
    {
        if (_index >= _history.Count - 1)
        {
            return;
        }

        _index++;
        GoBackCommand.RaiseCanExecuteChanged();
        GoForwardCommand.RaiseCanExecuteChanged();
        await LoadTopicAsync(CurrentTopic);
    }

    private async Task LoadTopicAsync(HelpTopic topic)
    {
        var version = ++_loadVersion;

        var markdown = await _helpService.GetHelpMarkdownAsync(topic);
        if (markdown is null || version != _loadVersion)
        {
            return;
        }

        Title = HelpHtmlRenderer.GetTitle(markdown) ?? Translate("Help");
        Source = new HtmlWebViewSource { Html = HelpHtmlRenderer.Render(markdown, topic, _helpService, IsDarkTheme()) };
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
