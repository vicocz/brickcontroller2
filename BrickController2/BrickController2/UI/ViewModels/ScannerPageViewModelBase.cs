using BrickController2.CreationManagement.Sharing;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels;

public abstract class ScannerPageViewModelBase : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private string? _currentValue;
    private bool _currentValueValidity;
    private bool _isTorchOn;
    private int _processing;

    public ScannerPageViewModelBase(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
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

    public bool IsTorchOn
    {
        get => _isTorchOn;
        set
        {
            if (_isTorchOn != value)
            {
                _isTorchOn = value;
                RaisePropertyChanged();
            }
        }
    }

    public override void OnDisappearing()
    {
        // disable scanning
        IsCurrentValueValid = false;
        CurrentValue = default!;
        // switch off the torch (page disappearing and application sleep)
        IsTorchOn = false;

        base.OnDisappearing();
    }

    internal void OnBarcodeDetected(BarcodeResult[] results)
    {
        var value = results.FirstOrDefault()?.Value;

        // same code as already shown, or previous one still being processed -> drop the frame
        if (string.IsNullOrEmpty(value) || (value == _currentValue && IsCurrentValueValid) ||
            Interlocked.CompareExchange(ref _processing, 1, 0) != 0)
        {
            return;
        }

        var token = DisappearingToken;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                // page disappeared meanwhile - do not restore stale state
                if (token.IsCancellationRequested)
                {
                    return;
                }

                CurrentValue = value;
                try
                {
                    ValidateQrCode(value);
                    IsCurrentValueValid = true;
                }
                catch
                {
                    IsCurrentValueValid = false;
                }
                ImportCommand.RaiseCanExecuteChanged();
            }
            finally
            {
                Interlocked.Exchange(ref _processing, 0);
            }
        });
    }

    protected abstract IShareable ValidateQrCode(string qr);
    protected abstract Task<IShareable> ImportQrCodeAsync(string qr);

    protected abstract string DescribeFailure(Exception ex);

    private async Task ImportAsync()
    {
        try
        {
            var item = await ImportQrCodeAsync(CurrentValue!);

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
                DescribeFailure(ex),
                Translate("Ok"),
                DisappearingToken);
        }

        // clear imported code
        CurrentValue = default!;
        IsCurrentValueValid = false;
    }
}
