using BrickController2.PlatformServices.InputDeviceService;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices.Sensors;

namespace BrickController2.InputDeviceManagement.Sensors;

internal class InputSensorService : InputDeviceServiceBase<OrientationSensorController>
{
    private readonly IInputDeviceEventServiceInternal _deviceEventServiceInternal;

    public InputSensorService(IInputDeviceManagerService inputDeviceManagerService,
        IInputDeviceEventServiceInternal deviceEventServiceInternal,
        ILogger<InputSensorService> logger) 
        : base(inputDeviceManagerService, logger)
    {
        _deviceEventServiceInternal = deviceEventServiceInternal;
    }

    public override void Initialize()
    {
        var sensor = OrientationSensor.Default;
        if (sensor.IsSupported)
        {
            AddInputDevice(new OrientationSensorController(_deviceEventServiceInternal, sensor));
        }
    }

    public override void Stop()
    {
        while (TryRemoveInputDevice(out var controller))
        {
            _logger.LogDebug("Sensor controller has been removed InputDeviceId:{controllerId}", controller.InputDeviceId);
        }
    }
}
