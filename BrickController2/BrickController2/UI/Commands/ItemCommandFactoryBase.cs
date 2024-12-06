using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Commands;

internal abstract class ItemCommandFactoryBase<TModel> where TModel : class, IShareable
{
    protected ItemCommandFactoryBase
    (
        IDialogService dialogService,
        ITranslationService translationService,
        ISharingManager<TModel> sharingManager,
        ISharedFileStorageService sharedFileStorageService,
        INavigationService navigationService
    )
    {
        DialogService = dialogService;
        TranslationService = translationService;
        SharingManager = sharingManager;
        SharedFileStorageService = sharedFileStorageService;
        NavigationService = navigationService;
    }

    protected IDialogService DialogService { get; }
    protected ITranslationService TranslationService { get; }
    protected ISharingManager<TModel> SharingManager { get; }
    protected ISharedFileStorageService SharedFileStorageService { get; }
    protected INavigationService NavigationService { get; }

    protected string Translate(string key) => TranslationService.Translate(key);
    protected string Translate(string key, string extra) => Translate(key) + " " + extra;
    protected string Translate(string key, Exception ex) => Translate(key, ex.Message);

    protected abstract Task ExportItemAsync(TModel model, string fileName);

    protected abstract string GetItemDescription(TModel item);

    protected abstract string GetFailureDescription(Exception ex);

    protected async Task ShareToClipboardAsync(TModel item)
        => await SharingManager.ShareToClipboardAsync(item);

    protected async Task ShareAsJsonFileAsync(TModel item)
    {
        var jsonFile = await SharingManager.ShareAsJsonFileAsync(item, FileSystem.CacheDirectory);
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = item.Name,
            File = new ShareFile(jsonFile)
        });
    }

    protected async Task ShareAsTextAsync(TModel item)
    {
        var json = await SharingManager.ShareAsync(item);

        await Share.RequestAsync(new ShareTextRequest
        {
            Subject = item.Name,
            Text = json,
            Title = item.Name
        });
    }

    protected async Task ExportItemAsync(TModel item, CancellationToken token)
    {
        try
        {
            var filename = item.Name;
            var done = false;

            do
            {
                var result = await DialogService.ShowInputDialogAsync(
                    filename,
                    GetItemDescription(item),
                    Translate("Ok"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    fn => FileHelper.FilenameValidator(fn),
                    token);

                if (!result.IsOk)
                {
                    return;
                }

                filename = result.Result;
                var filePath = Path.Combine(SharedFileStorageService.SharedStorageDirectory!, $"{filename}.{FileHelper.CreationFileExtension}");

                if (!File.Exists(filePath) ||
                    await DialogService.ShowQuestionDialogAsync(
                        Translate("FileAlreadyExists"),
                        Translate("DoYouWantToOverWrite"),
                        Translate("Yes"),
                        Translate("No"),
                        token))
                {
                    try
                    {
                        await ExportItemAsync(item, filePath);
                        done = true;

                        await DialogService.ShowMessageBoxAsync(
                            Translate("ExportSuccessful"),
                            filePath,
                            Translate("Ok"),
                            token);
                    }
                    catch (Exception ex)
                    {
                        await DialogService.ShowMessageBoxAsync(
                            Translate("Error"),
                            GetFailureDescription(ex),
                            Translate("Ok"),
                            token);

                        return;
                    }
                }
            }
            while (!done);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected async Task NavigateToItemSharePageAsync<TViewModel>(TModel item)
        where TViewModel : SharePageViewModeBase<TModel>
    {
        try
        {
            await NavigationService.NavigateToAsync<TViewModel>(new((nameof(item), item)));
        }
        catch (OperationCanceledException)
        {
        }
    }
}

