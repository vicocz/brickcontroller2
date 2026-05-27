using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Services.Preferences;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices.Sensors;

namespace BrickController2.InputDeviceManagement.Sensors;

internal class InputSensorService : InputDeviceServiceBase<OrientationSensorController>, IInputDeviceService<OrientationSensorController>
{
    private const string TiltSensorEnabledKey = "TiltSensorEnabled";

    private readonly IInputDeviceEventServiceInternal _deviceEventServiceInternal;
    private readonly IPreferencesService _preferencesService;

    public InputSensorService(IInputDeviceManagerService inputDeviceManagerService,
        IInputDeviceEventServiceInternal deviceEventServiceInternal,
        IPreferencesService preferencesService,
        ILogger<InputSensorService> logger) 
        : base(inputDeviceManagerService, logger)
    {
        _deviceEventServiceInternal = deviceEventServiceInternal;
        _preferencesService = preferencesService;
    }

    private static IOrientationSensor Sensor => OrientationSensor.Default;

    public bool IsEnabled
    {
        get => _preferencesService.Get(TiltSensorEnabledKey, false); // by default OFF
        set => _preferencesService.Set(TiltSensorEnabledKey, value);
    }

    public bool IsSupported => Sensor.IsSupported;

    public override void Initialize()
    {
        if (!IsSupported || !IsEnabled)
        {
            return;
        }

        AddInputDevice(new OrientationSensorController(_deviceEventServiceInternal, Sensor));
    }

    public override void Stop()
    {
        while (TryRemoveInputDevice(out var controller))
        {
            _logger.LogDebug("Sensor controller has been removed InputDeviceId:{controllerId}", controller.InputDeviceId);
        }
    }
}
