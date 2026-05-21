using BrickController2.CreationManagement;

namespace BrickController2.Extensions;

public static class ChannelOutputTypeExtensions
{
    public static bool IsChannelSetupSupported(this ChannelOutputType outputType) =>
        outputType == ChannelOutputType.ServoMotor ||
        outputType == ChannelOutputType.StepperMotor;
}
