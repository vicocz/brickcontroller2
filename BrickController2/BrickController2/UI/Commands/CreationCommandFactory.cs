using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

internal class CreationCommandFactory : ICommandFactory<Creation>
{
    private readonly IDialogService _dialogService;
    private readonly ITranslationService _translationService;
    private readonly ISharingManager<Creation> _sharingManager;
    private readonly ICreationManager _creationManager;
    private readonly INavigationService _navigationService;

    public CreationCommandFactory
    (
        IDialogService dialogService,
        ITranslationService translationService,
        ISharingManager<Creation> sharingManager,
        ICreationManager creationManager,
        INavigationService navigationService
    )
    {
        _dialogService = dialogService;
        _translationService = translationService;
        _sharingManager = sharingManager;
        _creationManager = creationManager;
        _navigationService = navigationService;
    }

    public ICommand CreateShareToClipboardCommand(Creation creation)
        => new SafeCommand(() => ShareToClipboardAsync(creation));
    public ICommand CreateShareAsJsonFileCommand(Creation creation)
        => new SafeCommand(() => ShareAsJsonFileAsync(creation));
    public ICommand CreateShareAsTextCommand(Creation creation)
        => new SafeCommand(() => ShareAsTextAsync(creation));
    public ICommand CreateNavigateToSharePageCommand(Creation creation)
    => new SafeCommand(() => NavigateToCreationSharePageAsync(creation));

    private async Task ShareToClipboardAsync(Creation creation)
        => await _sharingManager.ShareToClipboardAsync(creation);

    private async Task ShareAsJsonFileAsync(Creation creation)
    {
        var jsonFile = await _sharingManager.ShareAsJsonFileAsync(creation, FileSystem.CacheDirectory);
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = creation.Name,
            File = new ShareFile(jsonFile)
        });
    }

    private async Task ShareAsTextAsync(Creation creation)
    {
        var json = await _sharingManager.ShareAsync(creation);

        await Share.RequestAsync(new ShareTextRequest
        {
            Subject = creation.Name,
            Text = json,
            Title = creation.Name
        });
    }

    private async Task NavigateToCreationSharePageAsync(Creation creation)
    {
        try
        {
            await _navigationService.NavigateToAsync<CreationSharePageViewModel>(new NavigationParameters(("item", creation)));
        }
        catch (OperationCanceledException)
        {
        }
    }
}
