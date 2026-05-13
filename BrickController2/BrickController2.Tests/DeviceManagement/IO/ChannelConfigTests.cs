using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.IO;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.IO;

public class ChannelConfigTests
{
    [Fact]
    public void From_ServoMotor_ReturnsCorrectConfig()
    {
        var config = new ChannelConfiguration
        {
            ChannelOutputType = ChannelOutputType.ServoMotor,
            MaxServoAngle = 90,
            ServoBaseAngle = 45
        };

        var result = ChannelConfig.From(config);

        result.OutputType.Should().Be(ChannelOutputType.ServoMotor);
        result.MaxServoAngle.Should().Be(90);
        result.ServoBaseAngle.Should().Be(45);
        result.StepperAngle.Should().Be(0);
    }

    [Fact]
    public void From_StepperMotor_ReturnsCorrectConfig()
    {
        var config = new ChannelConfiguration
        {
            ChannelOutputType = ChannelOutputType.StepperMotor,
            StepperAngle = 30
        };

        var result = ChannelConfig.From(config);

        result.OutputType.Should().Be(ChannelOutputType.StepperMotor);
        result.StepperAngle.Should().Be(30);
        result.MaxServoAngle.Should().Be(0);
        result.ServoBaseAngle.Should().Be(0);
    }

    [Fact]
    public void From_Default_ReturnsEmptyConfig()
    {
        var config = new ChannelConfiguration
        {
            ChannelOutputType = ChannelOutputType.NormalMotor
        };

        var result = ChannelConfig.From(config);

        result.OutputType.Should().Be(ChannelOutputType.NormalMotor);
        result.MaxServoAngle.Should().Be(0);
        result.ServoBaseAngle.Should().Be(0);
        result.StepperAngle.Should().Be(0);
    }
}