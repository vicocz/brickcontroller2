// Built-in settings row "Orientation Sensor": switch that enables the device orientation sensor as input.
using BrickController2.InputDeviceManagement.Sensors;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// Toggle row for the orientation sensor; only visible when the device has the sensor.
/// Registered in Autofac as <see cref="ISettingsItem"/>.
/// </summary>
public class OrientationSensorSettingsItem : ToggleSettingsItem
{
    /// <summary>
    /// Creates the row.
    /// </summary>
    public OrientationSensorSettingsItem(
        IInputDeviceService<OrientationSensorController> sensorService,
        ITranslationService translationService)
        : base(
            translationService.Translate("OrientationSensor"),
            () => sensorService.IsSupported,
            () => sensorService.IsEnabled,
            value => sensorService.IsEnabled = value)
    {
    }

    /// <inheritdoc />
    public override int Order => 20;
}
