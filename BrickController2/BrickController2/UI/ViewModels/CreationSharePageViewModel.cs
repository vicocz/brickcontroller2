using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels;

public class CreationSharePageViewModel : SharePageViewModeBase<Creation>
{
    public CreationSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ISharingManager<Creation> sharingManager,
        ICommandFactory<Creation> commandFactory,
        NavigationParameters parameters)
        : base(navigationService, translationService, sharingManager, commandFactory, parameters)
    {
    }
}
