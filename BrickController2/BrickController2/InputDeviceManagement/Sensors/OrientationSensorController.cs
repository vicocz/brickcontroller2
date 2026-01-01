using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Linq;
using System.Numerics;

namespace BrickController2.InputDeviceManagement.Sensors;

internal class OrientationSensorController : InputDeviceBase<IOrientationSensor>
{
    private const double InversePI = 1.0 / Math.PI;
    private const double InverseHalfPI = 1.0 / (Math.PI / 2.0);

    public OrientationSensorController(IInputDeviceEventServiceInternal service,
        IOrientationSensor sensor)
        : base(service, sensor)
    {
        Name = "Orientation";
        InputDeviceId = nameof(OrientationSensor);
    }

    public override void Start()
    {
        base.Start();
        // Turn on orientation
        InputDeviceDevice.ReadingChanged += Orientation_ReadingChanged;
        InputDeviceDevice.Start(SensorSpeed.Game);
    }

    public override void Stop()
    {
        // Turn off orientation sensor
        InputDeviceDevice.ReadingChanged -= Orientation_ReadingChanged;
        InputDeviceDevice.Stop();
        base.Stop();
    }

    private void Orientation_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
    {
        var (Pitch, Roll, Yaw) = QuaternionToEulerAngles(e.Reading.Orientation);

        RaiseEvents(
        [
            (nameof(Pitch), (float)Pitch),
            (nameof(Roll), (float)Roll),
            (nameof(Yaw), (float)Yaw)
        ]);
    }

    private void RaiseEvents((string eventName, float value)[] axisEvents)
    {
        var events = axisEvents
            .Where(e => HasValueChanged(e.eventName, e.value))
            .ToDictionary(e => (InputDeviceEventType.Axis, e.eventName), e => e.value);

        RaiseEvent(events);
    }

    private static (float Pitch, float Roll, float Yaw) QuaternionToEulerAngles(Quaternion q)
    {
        // X-Axis (Pitch): Tilting the top of the phone forward/backward
        var sinPitch = 2.0 * (q.W * q.X + q.Y * q.Z);
        var cosPitch = 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
        var pitch = Math.Atan2(sinPitch, cosPitch);

        // Y-Axis (Roll): Tilting the phone left/right (banking)
        var sinRoll = 2.0 * (q.W * q.Y - q.Z * q.X);
        var roll = Math.Abs(sinRoll) >= 1
            ? Math.CopySign(Math.PI / 2, sinRoll) // Use 90 degrees if out of range
            : Math.Asin(sinRoll);

        // Z-Axis (Yaw/Azimuth): Rotating the phone like a compass on a table.
        // normalize radians to percentage for gamepad axis compatibility
        var sinYaw = 2.0 * (q.W * q.Z + q.X * q.Y);
        var cosYaw = 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
        var yaw = Math.Atan2(sinYaw, cosYaw);

        return (
            (float)(pitch * InversePI), // <-180; 180> => <-1; 1>
            (float)(roll * InverseHalfPI), // <-90; 90> => <-1; 1>
            (float)(yaw * InversePI) // <-180; 180> => <-1; 1>
        );
    }
}
