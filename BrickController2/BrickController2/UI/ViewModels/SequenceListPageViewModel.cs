﻿using BrickController2.CreationManagement;
using BrickController2.CreationManagement.Sharing;
using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class SequenceListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly ISharingManager<Sequence> _sharingManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _disappearingTokenSource;

        public SequenceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            ISharingManager<Sequence> sharingManager,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _sharingManager = sharingManager;
            _dialogService = dialogService;
            SharedFileStorageService = sharedFileStorageService;

            ImportSequenceCommand = new SafeCommand(async () => await ImportSequenceAsync(), () => SharedFileStorageService.IsSharedStorageAvailable);
            PasteSequenceCommand = new SafeCommand(PasteSequenceAsync);
            AddSequenceCommand = new SafeCommand(async () => await AddSequenceAsync());
            SequenceTappedCommand = new SafeCommand<Sequence>(async sequence => await NavigationService.NavigateToAsync<SequenceEditorPageViewModel>(new NavigationParameters(("sequence", sequence))));
            DeleteSequenceCommand = new SafeCommand<Sequence>(async (sequence) => await DeleteSequenceAsync(sequence));
        }

        public ObservableCollection<Sequence> Sequences => _creationManager.Sequences;

        public ISharedFileStorageService SharedFileStorageService { get; }

        public ICommand ImportSequenceCommand { get; }
        public ICommand PasteSequenceCommand { get; }
        public ICommand AddSequenceCommand { get; }
        public ICommand SequenceTappedCommand { get; }
        public ICommand DeleteSequenceCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource.Cancel();
        }

        private async Task ImportSequenceAsync()
        {
            try
            {
                var sequenceFilesMap = FileHelper.EnumerateDirectoryFilesToFilenameMap(SharedFileStorageService.SharedStorageDirectory, $"*.{FileHelper.SequenceFileExtension}");
                if (sequenceFilesMap?.Any() ?? false)
                {
                    var result = await _dialogService.ShowSelectionDialogAsync(
                        sequenceFilesMap.Keys,
                        Translate("Sequences"),
                        Translate("Cancel"),
                        _disappearingTokenSource.Token);

                    if (result.IsOk)
                    {
                        try
                        {
                            await _creationManager.ImportSequenceAsync(sequenceFilesMap[result.SelectedItem]);
                        }
                        catch (Exception)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Error"),
                                Translate("FailedToImportSequence"),
                                Translate("Ok"),
                                _disappearingTokenSource.Token);
                        }
                    }
                }
                else
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Information"),
                        Translate("NoSequencesToImport"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PasteSequenceAsync()
        {
            try
            {
                var sequence = await _sharingManager.ImportFromClipboardAsync();
                await _creationManager.ImportSequenceAsync(sequence);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Error"),
                    Translate("FailedToImportSequence", ex),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }
        }

        private async Task AddSequenceAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    null,
                    Translate("SequenceName"),
                    Translate("Create"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (sequenceName) => !string.IsNullOrEmpty(sequenceName),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("SequenceNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }
                    else if (!(await _creationManager.IsSequenceNameAvailableAsync(result.Result)))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("SequenceNameIsUsed"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    Sequence sequence = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            sequence = await _creationManager.AddSequenceAsync(result.Result);
                        },
                        Translate("Creating"),
                        token: _disappearingTokenSource.Token);

                    await NavigationService.NavigateToAsync<SequenceEditorPageViewModel>(new NavigationParameters(("sequence", sequence)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteSequenceAsync(Sequence sequence)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteSequence")} '{sequence.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteSequenceAsync(sequence),
                        Translate("Deleting"),
                        token: _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
