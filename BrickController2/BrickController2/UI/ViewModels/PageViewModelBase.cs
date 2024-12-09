using BrickController2.Helpers;
using BrickController2.UI.Commands;
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

        protected INavigationService NavigationService { get; }
        protected ITranslationService TranslationService { get; }

        protected internal CancellationToken DisappearingToken => _disappearingTokenSource?.Token ?? default;

        protected string Translate(string key) => TranslationService.Translate(key);
        protected string Translate(string key, string extra) => Translate(key) + " " + extra;
        protected string Translate(string key, Exception ex) => Translate(key, ex.Message);
    }
}
