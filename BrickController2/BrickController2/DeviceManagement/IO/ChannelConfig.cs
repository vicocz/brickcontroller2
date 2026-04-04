using BrickController2.CreationManagement;

namespace BrickController2.DeviceManagement.IO;

internal readonly record struct ChannelConfig
{
    public ChannelOutputType ChannelOutputType { get; init; }
    public int MaxServoAngle { get; init; }
    public int ServoBaseAngle { get; init; }
    public int StepperAngle { get; init; }
}
