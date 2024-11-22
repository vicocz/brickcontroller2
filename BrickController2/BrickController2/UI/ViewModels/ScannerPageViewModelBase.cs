using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels
{
    public abstract class ScannerPageViewModelBase<TModel> : PageViewModelBase where TModel : class, IShareable
    {
        private readonly IDialogService _dialogService;
        private string? _currentValue;
        private bool _currentValueValidity;
        private CancellationTokenSource? _disappearingTokenSource;

        protected readonly ISharingManager<TModel> SharingManager;

        public ScannerPageViewModelBase(
            INavigationService navigationService,
            ITranslationService translationService,
            ISharingManager<TModel> sharingManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            SharingManager = sharingManager;
            _dialogService = dialogService;

            ImportCommand = new SafeCommand(ImportAsync, () => IsCurrentValueValid);
        }

        public string? CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (_currentValue != value)
                {
                    _currentValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsCurrentValueValid
        {
            get { return _currentValueValidity; }
            set
            {
                _currentValueValidity = value;
                RaisePropertyChanged();
            }
        }

        public BarcodeReaderOptions Options { get; } = new BarcodeReaderOptions
        {
            AutoRotate = false,
            Formats = BarcodeFormat.QrCode,
            Multiple = false,
            TryHarder = false
        };

        public BarcodeFormat Format => BarcodeFormat.QrCode;

        public ICommand ImportCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            // disable scanning
            IsCurrentValueValid = false;

            _disappearingTokenSource?.Cancel();
        }

        internal void OnBarcodeDetected(BarcodeResult[] results)
        {
            // update preview
            CurrentValue = results.First().Value;
            // update validity
            try
            {
                SharingManager.Import(CurrentValue);
                IsCurrentValueValid = true;
            }
            catch
            {
                IsCurrentValueValid = false;
            }
            // update button availability
            MainThread.BeginInvokeOnMainThread(() => ImportCommand.RaiseCanExecuteChanged());
        }

        protected CancellationToken DisappearingToken => _disappearingTokenSource?.Token ?? default;

        protected abstract string ImportFailureWarning { get; }

        protected abstract Task ImportItemAsync(TModel model);

        private async Task ImportAsync()
        {
            try
            {
                var item = SharingManager.Import(CurrentValue!);
                await ImportItemAsync(item);

                await _dialogService.ShowMessageBoxAsync(
                    Translate("Import"),
                    Translate("ImportSuccessful", item.Name),
                    Translate("Ok"),
                    DisappearingToken);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Error"),
                    Translate("ImportFailureWarning", ex),
                    Translate("Ok"),
                    DisappearingToken);
            }

            // clear imported code
            CurrentValue = default!;
            IsCurrentValueValid = false;
        }
    }
}
