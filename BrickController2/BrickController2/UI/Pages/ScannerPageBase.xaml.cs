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
using Microsoft.Maui.Devices;

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
        Unloaded += (_, _) => ReleaseCamera();
    }

    private void ReleaseCamera()
    {
        _viewModel.IsTorchOn = false;
        CameraView.IsDetecting = false;
        // releases the native capture session (important on iOS)
        CameraView.Handler?.DisconnectHandler();
    }

    private async Task InitializeCamerasAsync(CancellationToken token)
    {
        try
        {
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

            if (_cameras.Count > 1)
            {
                var preferredLocation = _preferencesService.Get(CameraLocationKey, CameraLocation.Rear, PreferencesSection);
                // Fallback to index 0 if preferred location isn't found
                _cameraIndex = Math.Max(0, _cameras.FindIndex(c => c.Location == preferredLocation));
                await ApplyCameraAsync(_cameras[_cameraIndex], token);
            }
            else
            {
                CameraView.IsDetecting = true;
            }

            UpdateTorchState();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _cameras.Clear();
            SwitchCameraButton.IsVisible = false;

            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void SwitchCameraClicked(object sender, EventArgs e)
    {
        if (_cameras.Count < 2)
        {
            return;
        }

        try
        {
            _cameraIndex = (_cameraIndex + 1) % _cameras.Count;
            _viewModel.IsTorchOn = false;
            await ApplyCameraAsync(_cameras[_cameraIndex], _viewModel.DisappearingToken);
            UpdateTorchState();
            _preferencesService.Set(CameraLocationKey, _cameras[_cameraIndex].Location, PreferencesSection);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ApplyCameraAsync(CameraInfo camera, CancellationToken token)
    {
        // stop, reconfigure, start - iOS cannot swap inputs reliably while running
        CameraView.IsDetecting = false;
        await Task.Delay(DeviceInfo.Platform == DevicePlatform.iOS ? 300 : 0, token);

        CameraView.CameraLocation = camera.Location;
        CameraView.SelectedCamera = camera;
        CameraView.ZoomFactor = 0; // reset zoom, front camera may not support current value

        token.ThrowIfCancellationRequested();
        CameraView.IsDetecting = true;
    }

    private void UpdateTorchState()
    {
        var isRear = _cameras.Count > 0 &&
            CameraView?.SelectedCamera is not null &&
            CameraView.SelectedCamera.Location == CameraLocation.Rear;

        // a single front camera has no torch at all, so hide the button
        TorchButton.IsVisible = isRear || _cameras.Exists(x => x.Location == CameraLocation.Rear);
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
        _viewModel.OnBarcodeDetected(e.Results);
    }
}