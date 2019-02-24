using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class CameraSetupPageViewModel : PageViewModelBase
    {
        private readonly IDialogService _dialogService;

        private string _videoUrl;
        private CancellationTokenSource _disappearingTokenSource;

        public CameraSetupPageViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            ITranslationService translationService)
            : base(navigationService, translationService)
        {
            _dialogService = dialogService;

            _videoUrl = "rtsp://192.168.0.46:8554/1.3gp"; // TODO: temp

            EditVideoUrlCommand = new SafeCommand(async () => await EditVideoUrlAsync());
        }

        public ICommand EditVideoUrlCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource?.Cancel();
        }

        private async Task EditVideoUrlAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Translate("Edit"),
                    Translate("EnterVideoUrl"),
                    _videoUrl,
                    Translate("CameraVideoUrl"),
                    Translate("Edit"),
                    Translate("Cancel"),
                    _disappearingTokenSource.Token);
                if (result.IsOk)
                {
                    _videoUrl = result.Result;
                    // TODO: start video here
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
