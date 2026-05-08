using BrickController2.CreationManagement;

namespace BrickController2.DeviceManagement.IO;

/// <summary>
/// Describes configuration of a single channel.
/// This is used to determine how to control the channel.
/// </summary>
internal readonly record struct ChannelConfig
{
    public ChannelOutputType OutputType { get; init; }
    public int MaxServoAngle { get; init; }
    public int ServoBaseAngle { get; init; }
    public int StepperAngle { get; init; }
}
