using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.ApplicationModel;

namespace BrickController2.UI.ViewModels
{
    public class AboutPageViewModel : PageViewModelBase
    {
        private readonly IAppInfo _appInfo = AppInfo.Current;

        public AboutPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService)
            : base(navigationService, translationService)
        {
        }

        public string Version => _appInfo.VersionString;
    }
}
