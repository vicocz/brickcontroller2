using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Windows.PlatformServices.GameController;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.Dialog;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml.Controls;
using ProgressBar = Windows.UI.Xaml.Controls.ProgressBar;
using Windows.UI.Core;
using BrickController2.Windows.Extensions;
using Windows.Storage.Pickers;
using System.IO;
using Windows.Storage;

namespace BrickController2.Windows.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly GameControllerService _gameControllerService;

        public DialogService(GameControllerService gameControllerService)
        {
            _gameControllerService = gameControllerService;
        }

        private AlertDialog Create(string title, object content, string buttonText)
        {
            return new AlertDialog
            {
                Title = title,
                Content = content,

                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = buttonText,
            };
        }

        public async Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token)
        {
            string primaryButtonText = buttonText ?? "Ok";

            var dialog = Create(title, message, primaryButtonText);
            dialog.PrimaryButtonCommand = new Command(() =>
            {
                dialog.Hide();
            });

            using (token.Register(() =>
            {
                dialog.Hide();
            }))
            {
                await dialog.ShowAsync();
            }
        }

        public async Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var dialog = Create(title, message, positiveButtonText);


            dialog.PrimaryButtonCommand = new Command(() =>
            {
                dialog.Hide();
                completionSource.SetResult(true);
            });
            dialog.CloseButtonText = negativeButtonText;
            dialog.CloseButtonCommand = new Command(() =>
            {
                dialog.Hide();
                completionSource.SetResult(false);
            });

            using (token.Register(() =>
            {
                dialog.Hide();
                completionSource.SetResult(false);
            }))
            {
                await dialog.ShowAsync();
            }

            return await completionSource.Task;
        }

        public async Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<InputDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            var text = initialValue ?? string.Empty;
            var textBox = new TextBox
            {
                Text = text,
                SelectionLength = text.Length,
                //Hint = placeHolder ?? string.Empty
            };

            var dialog = Create(title, textBox, positiveButtonText ?? "Ok");
            dialog.PrimaryButtonCommand = new Command(() =>
            {
                dialog.Hide();
                completionSource.SetResult(new InputDialogResult(true, textBox.Text));
            });
            dialog.CloseButtonText = negativeButtonText ?? "Cancel";
            dialog.CloseButtonCommand = new Command(() =>
            {
                dialog.Hide();
                completionSource.SetResult(new InputDialogResult(false, textBox.Text));
            });
            using (token.Register(() =>
            {
                dialog.Hide();
                completionSource.SetResult(new InputDialogResult(false, textBox.Text));
            }))
            {
                await dialog.ShowAsync();
            }

            return await completionSource.Task;
        }

        public async Task<InputDialogResult> ShowFileSaveDialogAsync(string title, string message, string initialName, string extension,
            Action<Stream> writer,
            CancellationToken token)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedFileName = initialName,
                DefaultFileExtension = extension,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            savePicker.FileTypeChoices.Add("json", new[] { extension });

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                // write to file

                using (var stream = new MemoryStream())
                {
                    writer(stream);
                    await FileIO.WriteBytesAsync(file, stream.ToArray());
                }

                var status = await CachedFileManager.CompleteUpdatesAsync(file);

                return new InputDialogResult(true, file.Path);
            }
            return new InputDialogResult(false, string.Empty);
        }

        public async Task<InputDialogResult> ShowFilePickerDialogAsync(string title, string message, string extension, Func<StreamReader, Task> fileReader, CancellationToken token)
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            openPicker.FileTypeFilter.Add(extension);

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var stream = await file.OpenReadAsync();

                // Convert the stream to a .NET stream using AsStream, pass to a 
                // StreamReader and read the stream.
                await fileReader(new StreamReader(stream.AsStream()));

                return new InputDialogResult(true, file.Path);
            }
            return new InputDialogResult(false, string.Empty);
        }

        public async Task ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title, string message, string cancelButtonText)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                var progressBar = new ProgressBar()
                {
                    IsIndeterminate = !isDeterministic,
                    Value = 0,
                    Minimum = 0,
                    Maximum = 100
                };

                var panel = new StackPanel();
                panel.Children.Add(progressBar);
                panel.Children.Add(new TextBlock { Text = message ?? "" });

                var dialog = new AlertDialog
                {
                    Title = title,
                    Content = panel
                };

                if (!string.IsNullOrEmpty(cancelButtonText))
                {
                    dialog.CloseButtonText = cancelButtonText;
                    dialog.CloseButtonCommand = new Command(() => tokenSource.Cancel());
                }

                void DialogCanceledHandler(ContentDialog sender, ContentDialogClosedEventArgs args) => tokenSource.Cancel();

                async Task CloseDialog()
                {
                    await dialog.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        dialog.Hide();
                    });
                }

                dialog.Closed += DialogCanceledHandler;
                var progressDialog = new ProgressDialog(dialog, progressBar);

                using (tokenSource.Token.Register(async () =>
                {
                    await CloseDialog();
                }))
                {
                    var actionTask = action(progressDialog, tokenSource.Token);

                    actionTask.ContinueWith((t) => CloseDialog())
                        .Forget();

                    await dialog.ShowAsync();
                }
            }
        }

        public async Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<GameControllerEventDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            var dialog = new GameControllerEventDialog(_gameControllerService)
            {
                Title = title,
                Content = message,
                CloseButtonText = cancelButtonText ?? "Cancel",
            };

            dialog.CloseButtonCommand = new Command(() =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Hide();
                completionSource.SetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Button, null));
            });

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            using (token.Register(() =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Hide();
                completionSource.SetCanceled();
            }))
            {
                await dialog.ShowAsync();
            }
            return await completionSource.Task;

            void GameControllerEventHandler(object sender, GameControllerEventArgs args)
            {
                if (args.ControllerEvents.Count == 0)
                {
                    return;
                }

                foreach (var controllerEvent in args.ControllerEvents)
                {
                    if ((controllerEvent.Key.EventType == GameControllerEventType.Axis && Math.Abs(controllerEvent.Value) > 0.8) ||
                        (controllerEvent.Key.EventType == GameControllerEventType.Button && Math.Abs(controllerEvent.Value) < 0.05))
                    {
                        _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                        dialog.Hide();
                        completionSource.SetResult(new GameControllerEventDialogResult(true, controllerEvent.Key.EventType, controllerEvent.Key.EventCode));
                        return;
                    }
                }
            }
        }
    }
}