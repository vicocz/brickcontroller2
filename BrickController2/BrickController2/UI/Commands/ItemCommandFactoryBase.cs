using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.Commands;

internal abstract class ItemCommandFactoryBase<TModel> : ICommandFactory<TModel>
    where TModel : class, IShareable
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

    public ICommand CreateShareToClipboardCommand(TModel item)
        => new SafeCommand(() => ShareToClipboardAsync(item));
    public ICommand CreateShareAsJsonFileCommand(TModel item)
        => new SafeCommand(() => ShareAsJsonFileAsync(item));
    public ICommand CreateShareAsTextCommand(TModel item)
        => new SafeCommand(() => ShareAsTextAsync(item));
    public ICommand CreateExportItemAsFileCommand(TModel item, CancellationToken token)
        => new SafeCommand(() => ExportItemAsync(item, token), () => SharedFileStorageService.IsSharedStorageAvailable);
    public ICommand CreateImportItemFromJsonFileCommand(CancellationToken token)
        => new SafeCommand(() => ImportItemFromJsonFileAsync(token));
    public ICommand CreateImportItemFromFileCommand(CancellationToken token)
        => new SafeCommand(() => ImportItemFromFileAsync(token));
    public ICommand CreatePasteItemFromClipboardCommand(CancellationToken token)
        => new SafeCommand(() => PasteItemFromClipboardAsync(token));

    protected IDialogService DialogService { get; }
    protected ITranslationService TranslationService { get; }
    protected ISharingManager<TModel> SharingManager { get; }
    protected ISharedFileStorageService SharedFileStorageService { get; }
    protected INavigationService NavigationService { get; }

    protected abstract string ItemNameHint { get; }
    protected abstract string ItemsTitle { get; }

    protected string Translate(string key) => TranslationService.Translate(key);
    protected string Translate(string key, string extra) => Translate(key) + " " + extra;
    protected string Translate(string key, Exception ex) => Translate(key, ex.Message);

    protected abstract Task ExportItemAsync(TModel model, string fileName);

    protected abstract Task ImportItemAsync(TModel model);

    protected abstract string GetExportFailureDescription(Exception ex);
    protected abstract string GetImportFailureDescription(Exception ex);

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
                    ItemNameHint,
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
                var filePath = Path.Combine(SharedFileStorageService.SharedStorageDirectory!, $"{filename}.{TModel.Type}");

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
                            GetExportFailureDescription(ex),
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

    protected async Task ImportItemFromJsonFileAsync(CancellationToken token)
    {
        try
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select a JSON file",
                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, ["public.json"] },
                        { DevicePlatform.Android, ["application/json"] },
                        { DevicePlatform.WinUI, [".json"] },
                    })
            };

            var result = await FilePicker.PickAsync(options);
            if (result != null)
            {
                try
                {
                    using var stream = await result.OpenReadAsync();
                    var item = await SharingManager.ImportFromJsonFileAsync(stream);
                    await ImportItemAsync(item);
                }
                catch (Exception ex)
                {
                    await DialogService.ShowMessageBoxAsync(
                        Translate("Error"),
                        GetImportFailureDescription(ex),
                        Translate("Ok"),
                        token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ImportItemFromFileAsync(CancellationToken token)
    {
        try
        {
            var itemFilesMap = FileHelper.EnumerateDirectoryFilesToFilenameMap(SharedFileStorageService.SharedStorageDirectory!, $"*.{TModel.Type}");
            var result = await DialogService.ShowSelectionDialogAsync(
                itemFilesMap.Keys,
                ItemsTitle,
                Translate("Cancel"),
                token);

            if (result.IsOk)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(itemFilesMap[result.SelectedItem], token);
                    var item = SharingManager.ImportWithoutValidation(json);
                    await ImportItemAsync(item);
                }
                catch (Exception ex)
                {
                    await DialogService.ShowMessageBoxAsync(
                        Translate("Error"),
                        GetImportFailureDescription(ex),
                        Translate("Ok"),
                        token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected async Task PasteItemFromClipboardAsync(CancellationToken token)
    {
        try
        {
            var item = await SharingManager.ImportFromClipboardAsync();
            await ImportItemAsync(item);
        }
        catch (Exception ex)
        {
            await DialogService.ShowMessageBoxAsync(
                Translate("Error"),
                GetImportFailureDescription(ex),
                Translate("Ok"),
                token);
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

