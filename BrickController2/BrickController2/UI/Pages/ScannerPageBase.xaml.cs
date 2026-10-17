using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.ViewModels;
using ZXing.Net.Maui;

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

        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e) =>
        // The native handler is now ready, Android will find the cameras
        await InitializeCamerasAsync(_viewModel.DisappearingToken);

    private void OnPageUnloaded(object? sender, EventArgs e)
    {
        _viewModel.IsTorchOn = false;
        CameraView.IsDetecting = false;
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

            if (_cameras.Count > 0)
            {
                var preferredLocation = _preferencesService.Get(CameraLocationKey, CameraLocation.Rear, PreferencesSection);
                // Fallback to index 0 if preferred location isn't found
                _cameraIndex = Math.Max(0, _cameras.FindIndex(c => c.Location == preferredLocation));
                await ApplyCameraAsync(_cameras[_cameraIndex], token);
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
        SetCurrentCamera(camera);
        CameraView.ZoomFactor = 0; // reset zoom, front camera may not support current value
        ZoomSlider.Value = 0;

        token.ThrowIfCancellationRequested();
        CameraView.IsDetecting = true;
    }

    private void SetCurrentCamera(CameraInfo camera)
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // iOS: location-based selection is the reliable path
            CameraView.CameraLocation = camera.Location;
        }
        else
        {
            CameraView.SelectedCamera = camera;
        }
    }

    private bool IsRearCameraActive()
    {
        if (_cameras.Count == 0 || CameraView is null)
        {
            return false;
        }

        // iOS: camera is selected by location (see SetCurrentCamera)
        var location = DeviceInfo.Platform == DevicePlatform.iOS
            ? CameraView.CameraLocation
            : CameraView.SelectedCamera?.Location;

        return location == CameraLocation.Rear;
    }

    private void UpdateTorchState()
    {
        var isRear = IsRearCameraActive();

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