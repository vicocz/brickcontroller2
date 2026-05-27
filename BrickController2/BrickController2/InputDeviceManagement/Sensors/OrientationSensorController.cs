using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BrickController2.InputDeviceManagement.Sensors;

public class OrientationSensorController : InputDeviceBase<IOrientationSensor>
{
    private const double InversePI = 1.0 / Math.PI;
    private const double InverseHalfPI = 1.0 / (Math.PI / 2.0);
    private const int RoundingPrecision = 4;

    private const string PitchName = "Pitch";
    private const string RollName = "Roll";
    private const string YawName = "Yaw";

    private static readonly (InputDeviceEventType, string) PitchKey = (InputDeviceEventType.Axis, PitchName);
    private static readonly (InputDeviceEventType, string) RollKey = (InputDeviceEventType.Axis, RollName);
    private static readonly (InputDeviceEventType, string) YawKey = (InputDeviceEventType.Axis, YawName);

    private readonly Dictionary<(InputDeviceEventType, string), float> _eventBuffer = new(3);

    public OrientationSensorController(IInputDeviceEventServiceInternal service,
        IOrientationSensor sensor)
        : base(service, sensor)
    {
        InputDeviceId = "Sensor";
        Name = "Tilt";
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
        var (pitch, roll, yaw) = QuaternionToEulerAngles(e.Reading.Orientation);

        _eventBuffer.Clear();

        if (HasValueChanged(PitchName, pitch))
            _eventBuffer[PitchKey] = pitch;
        if (HasValueChanged(RollName, roll))
            _eventBuffer[RollKey] = roll;
        if (HasValueChanged(YawName, yaw))
            _eventBuffer[YawKey] = yaw;

        RaiseEvent(_eventBuffer);
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
            (float)Math.Round(pitch * InversePI, RoundingPrecision), // <-180; 180> => <-1; 1>
            (float)Math.Round(roll * InverseHalfPI, RoundingPrecision), // <-90; 90> => <-1; 1>
            (float)Math.Round(yaw * InversePI, RoundingPrecision) // <-180; 180> => <-1; 1>
        );
    }
}
