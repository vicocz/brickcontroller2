using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels;

public class SequenceSharePageViewModel : SharePageViewModeBase<Sequence>
{
    public SequenceSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ISharingManager<Sequence> sharingManager,
        ICommandFactory<Sequence> commandFactory,
        NavigationParameters parameters)
        : base(navigationService, translationService, sharingManager, commandFactory, parameters)
    {
    }
}
