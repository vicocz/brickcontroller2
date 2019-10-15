namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// 
    /// </summary>
    /// <see href="https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#io-type-id"/>
    public enum DevicePortType
    {
        Unknown = 0x0000,
        Motor = 0x0001,
        SystemTrainMotor = 0x0002,
        Button = 0x0005,
        LedLight = 0x0008,

        Voltage = 0x0014,
        Current = 0x0015,
        PiezoTone = 0x0016,
        RgbLight = 0x0017,
        BoostLed = 0x0018,

        // WEDO2_TILT
        ExternalTiltSensor = 0x0022,

        // WEDO2_DISTANCE
        MotionSensor = 0x0023,

        BOOST_DISTANCE = 37,

        // BOOST_TACHO_MOTOR
        ExternalMotorWithTacho = 0x0026,

        // BOOST_MOVE_HUB_MOTOR
        InternalMotorWithTacho = 0x0027,

        // BOOST_TILT = 40,
        InternalTilt = 0x0028,

        DUPLO_TRAIN_BASE_MOTOR = 41,
        DUPLO_TRAIN_BASE_SPEAKER = 42,
        DUPLO_TRAIN_BASE_COLOR = 43,
        DUPLO_TRAIN_BASE_SPEEDOMETER = 44,
        CONTROL_PLUS_LARGE_MOTOR = 46,
        CONTROL_PLUS_XLARGE_MOTOR = 47,
        POWERED_UP_REMOTE_BUTTON = 55,
    }
}