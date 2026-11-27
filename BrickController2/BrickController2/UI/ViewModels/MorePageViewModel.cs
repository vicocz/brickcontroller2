using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels;

public class MorePageViewModel : PageViewModelBase
{
    public MorePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService)
        : base(navigationService, translationService)
    {
    }

    public override string? RootKey => RootKeys.More;
}
