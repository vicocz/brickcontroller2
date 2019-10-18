using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.IO;

namespace BrickController2.UI.ViewModels
{
    public class BlocksTesterPageViewModel : PageViewModelBase
    {
        public BlocksTesterPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService)
            : base(navigationService, translationService)
        {

            BaseUrl = RootNameSpace;

        }

        public static string RootNameSpace => "BrickController2.UI.Html";

        public string BaseUrl { get; }

        public string WebPageData { get; }

    }
}
