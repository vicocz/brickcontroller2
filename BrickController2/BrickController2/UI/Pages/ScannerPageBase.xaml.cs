using BrickController2.UI.Services.Background;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.Controls.Xaml;
using ZXing.Net.Maui;
using System.Linq;

namespace BrickController2.UI.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ScannerPageBase
{
    private const string PreferencesSection = "Scanner";
    private const string CameraLocationKey = "CameraLocation";

    private readonly IPreferencesService _preferencesService;
    private readonly List<CameraInfo> _cameras = [];
    private readonly ScannerPageViewModelBase _viewModel;
    private int _cameraIndex;

    public ScannerPageBase(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost, IPreferencesService preferencesService)
        : base(backgroundService, dialogServerHost)
    {
        _viewModel = (ScannerPageViewModelBase)vm;
        _preferencesService = preferencesService;
        InitializeComponent();
        AfterInitialize(vm);
        Loaded += async (_, _) => await InitializeCamerasAsync(vm.DisappearingToken);
    }

    private async Task InitializeCamerasAsync(CancellationToken token)
    {
        try
        {
            // the view may need a moment
            for (var attempt = 0; attempt < 10 && !token.IsCancellationRequested; attempt++)
            {
                _cameras.Clear();
                _cameras.AddRange((await CameraView.GetAvailableCameras()) ?? []);
                if (_cameras.Count > 0 || !IsLoaded)
                {
                    break;
                }

                await Task.Delay(500, token);
            }

            token.ThrowIfCancellationRequested();

            SwitchCameraButton.IsVisible = _cameras.Count > 1;

            var preferredLocation = _preferencesService.Get(CameraLocationKey, CameraLocation.Rear, PreferencesSection);
            var selectedIndex = Math.Max(0, _cameras.FindIndex(c => c.Location == preferredLocation));

            if (_cameras.Count > 0)
            {
                // always align the view with the retrieved cameras, the default location might not be available
                _cameraIndex = selectedIndex;
                CameraView.SelectedCamera = _cameras[_cameraIndex];
            }

            UpdateTorchState();
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            _cameras.Clear();
            SwitchCameraButton.IsVisible = false;
        }
    }

    private void SwitchCameraClicked(object sender, EventArgs e)
    {
        if (_cameras.Count < 2)
        {
            return;
        }

        _cameraIndex = (_cameraIndex + 1) % _cameras.Count;
        _viewModel.IsTorchOn = false;
        CameraView.SelectedCamera = _cameras[_cameraIndex];
        UpdateTorchState();
        _preferencesService.Set(CameraLocationKey, _cameras[_cameraIndex].Location, PreferencesSection);
    }

    private void UpdateTorchState()
    {
        var hasCamera = _cameras.Count > 0;
        var isRear = hasCamera && CameraView.SelectedCamera.Location == CameraLocation.Rear;

        // a single front camera has no torch at all, so hide the button
        TorchButton.IsVisible = isRear || _cameras.Any(x => x.Location == CameraLocation.Rear);
        TorchButton.IsEnabled = isRear;

        if (!isRear)
        {
            _viewModel.IsTorchOn = false;
        }
    }

    private void TorchClicked(object sender, EventArgs e)
    {
        _viewModel.IsTorchOn = !_viewModel.IsTorchOn;
    }

    private void ZoomChanged(object sender, ValueChangedEventArgs e)
    {
        CameraView.ZoomFactor = (float)e.NewValue;
    }

    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (BindingContext is ScannerPageViewModelBase viewModel)
        {
            viewModel.OnBarcodeDetected(e.Results);
        }
    }
}