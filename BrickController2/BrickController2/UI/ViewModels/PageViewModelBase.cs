using BrickController2.Helpers;
using BrickController2.UI.Commands;
using BrickController2.UI.Extensions;
using BrickController2.UI.Services.Help;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public abstract class PageViewModelBase : NotifyPropertyChangedSource, IPageViewModel
    {
        private CancellationTokenSource? _disappearingTokenSource;

        protected PageViewModelBase(INavigationService navigationService, ITranslationService translationService)
        {
            NavigationService = navigationService;
            TranslationService = translationService;

            BackCommand = new SafeCommand(() => NavigationService.NavigateBackAsync());
            ShowHelpCommand = new SafeCommand(
                () => NavigationService.NavigateToAsync<HelpPageViewModel>(new NavigationParameters(("topic", GetHelpTopic()!))),
                () => HasHelp);
        }

        public virtual void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public virtual void OnDisappearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = null;
        }

        public virtual bool OnBackButtonPressed() => true;

        public ICommand BackCommand { get; }

        public ICommand ShowHelpCommand { get; }

        public bool HasHelp => GetHelpTopic() is { } topic && HelpResources.Exists(topic);

        /// <summary>
        /// Help topic of the page. By default derived from the view model name (e.g. CreationListPageViewModel => pages/creation-list).
        /// </summary>
        protected virtual HelpTopic? GetHelpTopic() => HelpTopic.ForPage(GetType().Name);

        protected INavigationService NavigationService { get; }
        protected ITranslationService TranslationService { get; }

        protected internal CancellationToken DisappearingToken => _disappearingTokenSource?.Token ?? default;

        protected string Translate(string key) => TranslationService.Translate(key);
        protected string Translate(string key, string extra) => TranslationService.Translate(key, extra);
        protected string Translate(string key, Exception ex) => TranslationService.Translate(key, ex);
    }
}
