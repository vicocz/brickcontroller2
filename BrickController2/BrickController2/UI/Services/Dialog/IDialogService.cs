using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Dialog
{
    public interface IDialogService
    {
        Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token);

        Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token);

        Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, CancellationToken token);

        Task<InputDialogResult> ShowFileSaveDialogAsync(string title, string message, string initialName, string extension,
            Action<Stream> writer,
            CancellationToken token);
        Task<InputDialogResult> ShowFilePickerDialogAsync(string title, string message, string extension, Func<StreamReader, Task> stream, CancellationToken token);

        Task ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title = null, string message = null, string cancelButtonText = null);

        Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token);
    }
}
