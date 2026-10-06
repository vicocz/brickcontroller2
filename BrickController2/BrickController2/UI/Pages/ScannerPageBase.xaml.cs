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

namespace BrickController2.UI.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ScannerPageBase
{
    private const string PreferencesSection = "Scanner";
    private const string CameraLocationKey = "CameraLocation";

    private readonly IPreferencesService _preferencesService;
    private readonly List<CameraInfo> _cameras = [];
    private int _cameraIndex;


    public ScannerPageBase(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost, IPreferencesService preferencesService)
        : base(backgroundService, dialogServerHost)
    {
        _preferencesService = preferencesService;
        InitializeComponent();
        AfterInitialize(vm);
        Loaded += async (_, _) => await InitializeCamerasAsync(vm.DisappearingToken);
    }

    protected override void OnDisappearing()
    {
        CameraView.IsTorchOn = false;
        base.OnDisappearing();
    }

    private async Task InitializeCamerasAsync(CancellationToken token)
    {
        try
        {
            // on the first display the camera permission may not be granted yet, so the list can be empty - retry for a while
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

            var preferedLocation = _preferencesService.Get(CameraLocationKey, CameraLocation.Rear, PreferencesSection);
            var selectedIndex = Math.Max(0, _cameras.FindIndex(c => c.Location == preferedLocation));

            if (_cameras.Count > 0)
            {
                // always align the view with the retrieved cameras, the default location might not be available
                _cameraIndex = selectedIndex;
                CameraView.CameraLocation = _cameras[selectedIndex].Location;
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
        CameraView.IsTorchOn = false;
        CameraView.CameraLocation = _cameras[_cameraIndex].Location;
        UpdateTorchState();
        _preferencesService.Set(CameraLocationKey, _cameras[_cameraIndex].Location, PreferencesSection);
    }

    private void UpdateTorchState()
    {
        var isRear = CameraView.CameraLocation == CameraLocation.Rear;

        // a single front camera has no torch at all, so hide the button
        TorchButton.IsVisible = isRear || _cameras.Count != 1;
        TorchButton.IsEnabled = isRear;

        if (!isRear)
        {
            CameraView.IsTorchOn = false;
        }
    }

    private void TorchClicked(object sender, EventArgs e)
    {
        CameraView.IsTorchOn = !CameraView.IsTorchOn;
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