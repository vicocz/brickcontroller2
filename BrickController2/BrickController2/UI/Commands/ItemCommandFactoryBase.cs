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

    public ICommand ShareToClipboardCommand(PageViewModelBase viewModel, TModel item)
        => new SafeCommand(() => ShareToClipboardAsync(item));
    public ICommand ShareAsJsonFileCommand(PageViewModelBase viewModel, TModel item)
        => new SafeCommand(() => ShareAsJsonFileAsync(item));
    public ICommand ShareAsTextCommand(PageViewModelBase viewModel, TModel item)
        => new SafeCommand(() => ShareAsTextAsync(item));
    public ICommand ExportItemAsFileCommand(PageViewModelBase viewModel, TModel item)
        => new SafeCommand(() => ExportItemAsync(item, viewModel.DisappearingToken), () => SharedFileStorageService.IsSharedStorageAvailable);
    public ICommand ImportItemFromJsonFileCommand(PageViewModelBase viewModel)
        => new SafeCommand(() => ImportItemFromJsonFileAsync(viewModel.DisappearingToken));
    public ICommand ImportItemFromFileCommand(PageViewModelBase viewModel)
        => new SafeCommand(() => ImportItemFromFileAsync(viewModel.DisappearingToken), () => SharedFileStorageService.IsSharedStorageAvailable);
    public ICommand PasteItemFromClipboardCommand(PageViewModelBase viewModel)
        => new SafeCommand(() => PasteItemFromClipboardAsync(viewModel.DisappearingToken));

    protected IDialogService DialogService { get; }
    protected ITranslationService TranslationService { get; }
    protected ISharingManager<TModel> SharingManager { get; }
    protected ISharedFileStorageService SharedFileStorageService { get; }
    protected INavigationService NavigationService { get; }

    protected abstract string ItemNameHint { get; }
    protected abstract string ItemsTitle { get; }
    protected abstract string NoItemToImportMessage { get; }

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
        var json = await SharingManager.ShareAsync(item, compact: false);

        string filePath = Path.Combine(FileSystem.CacheDirectory, $"{item.Name}.json");
        await File.WriteAllTextAsync(filePath, json);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = item.Name,
            File = new ShareFile(filePath)
        });
    }

    protected async Task ShareAsTextAsync(TModel item)
    {
        var json = await SharingManager.ShareAsync(item, compact:false);

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
                    FileHelper.FilenameValidator,
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
                PickerTitle = Translate("ChooseJsonFileToImport"),
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
                    using StreamReader sr = new(stream);
                    var json = await sr.ReadToEndAsync();
                    var item = SharingManager.Import(json);
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

            if (itemFilesMap.Count == 0)
            {
                await DialogService.ShowMessageBoxAsync(
                    Translate("Information"),
                    NoItemToImportMessage,
                    Translate("Ok"),
                    token);
                return;
            }

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
}

