using System.Collections.Generic;
using BrickController2.PlatformServices.InputDevice;
using FluentAssertions;
using Moq;
using Xunit;

namespace BrickController2.Tests.PlatformServices.InputDevice;

public class InputDeviceConnectorExtensionsTests
{
    [Fact]
    public void RaiseAxisEventConditionally_WhenConnectorIsNull_ShouldReturnFalse()
    {
        // Arrange
        IInputDeviceConnector? connector = null;

        // Act
        var result = connector.RaiseAxisEventConditionally("X", 0.5f);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RaiseAxisEventConditionally_WhenValueHasNotChanged_ShouldReturnFalseAndNotRaiseEvent()
    {
        // Arrange
        var connectorMock = new Mock<IInputDeviceConnector>();
        connectorMock
            .Setup(c => c.HasValueChanged(InputDeviceEventType.Axis, "X", 0.5f))
            .Returns(false);

        // Act
        var result = connectorMock.Object.RaiseAxisEventConditionally("X", 0.5f);

        // Assert
        result.Should().BeFalse();
        connectorMock.Verify(c => c.RaiseEvent(It.IsAny<IReadOnlyDictionary<(InputDeviceEventType, string), float>>()), Times.Never);
    }

    [Fact]
    public void RaiseAxisEventConditionally_WhenValueHasChanged_ShouldReturnTrueAndRaiseEventWithAxisValue()
    {
        // Arrange
        var connectorMock = new Mock<IInputDeviceConnector>();
        connectorMock
            .Setup(c => c.HasValueChanged(InputDeviceEventType.Axis, "X", 0.5f))
            .Returns(true);

        IReadOnlyDictionary<(InputDeviceEventType, string), float>? raisedEvents = null;
        connectorMock
            .Setup(c => c.RaiseEvent(It.IsAny<IReadOnlyDictionary<(InputDeviceEventType, string), float>>()))
            .Callback<IReadOnlyDictionary<(InputDeviceEventType, string), float>>(events => raisedEvents = events);

        // Act
        var result = connectorMock.Object.RaiseAxisEventConditionally("X", 0.5f);

        // Assert
        result.Should().BeTrue();
        raisedEvents.Should().ContainSingle()
            .Which.Should().Be(new KeyValuePair<(InputDeviceEventType, string), float>((InputDeviceEventType.Axis, "X"), 0.5f));
    }

    [Fact]
    public void RaiseButtonEventConditionally_WhenConnectorIsNull_ShouldReturnFalse()
    {
        // Arrange
        IInputDeviceConnector? connector = null;

        // Act
        var result = connector.RaiseButtonEventConditionally("A", true);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, InputDevices.BUTTON_PRESSED)]
    [InlineData(false, InputDevices.BUTTON_RELEASED)]
    public void RaiseButtonEventConditionally_WhenValueHasChanged_ShouldRaiseEventWithExpectedValue(bool isPressed, float expectedValue)
    {
        // Arrange
        var connectorMock = new Mock<IInputDeviceConnector>();
        connectorMock
            .Setup(c => c.HasValueChanged(InputDeviceEventType.Button, "A", expectedValue))
            .Returns(true);

        IReadOnlyDictionary<(InputDeviceEventType, string), float>? raisedEvents = null;
        connectorMock
            .Setup(c => c.RaiseEvent(It.IsAny<IReadOnlyDictionary<(InputDeviceEventType, string), float>>()))
            .Callback<IReadOnlyDictionary<(InputDeviceEventType, string), float>>(events => raisedEvents = events);

        // Act
        var result = connectorMock.Object.RaiseButtonEventConditionally("A", isPressed);

        // Assert
        result.Should().BeTrue();
        raisedEvents.Should().ContainSingle()
            .Which.Should().Be(new KeyValuePair<(InputDeviceEventType, string), float>((InputDeviceEventType.Button, "A"), expectedValue));
    }

    [Fact]
    public void RaiseButtonEventConditionally_WhenValueHasNotChanged_ShouldReturnFalseAndNotRaiseEvent()
    {
        // Arrange
        var connectorMock = new Mock<IInputDeviceConnector>();
        connectorMock
            .Setup(c => c.HasValueChanged(InputDeviceEventType.Button, "A", InputDevices.BUTTON_PRESSED))
            .Returns(false);

        // Act
        var result = connectorMock.Object.RaiseButtonEventConditionally("A", true);

        // Assert
        result.Should().BeFalse();
        connectorMock.Verify(c => c.RaiseEvent(It.IsAny<IReadOnlyDictionary<(InputDeviceEventType, string), float>>()), Times.Never);
    }

    [Fact]
    public void RaiseEventConditionally_WhenConnectorIsNull_ShouldReturnFalse()
    {
        // Arrange
        IInputDeviceConnector? connector = null;

        // Act
        var result = connector.RaiseEventConditionally(InputDeviceEventType.Axis, "Y", 1.0f);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RaiseEventConditionally_WhenValueHasChanged_ShouldReturnTrueAndRaiseEventWithGivenTypeAndCode()
    {
        // Arrange
        var connectorMock = new Mock<IInputDeviceConnector>();
        connectorMock
            .Setup(c => c.HasValueChanged(InputDeviceEventType.Axis, "Y", 1.0f))
            .Returns(true);

        IReadOnlyDictionary<(InputDeviceEventType, string), float>? raisedEvents = null;
        connectorMock
            .Setup(c => c.RaiseEvent(It.IsAny<IReadOnlyDictionary<(InputDeviceEventType, string), float>>()))
            .Callback<IReadOnlyDictionary<(InputDeviceEventType, string), float>>(events => raisedEvents = events);

        // Act
        var result = connectorMock.Object.RaiseEventConditionally(InputDeviceEventType.Axis, "Y", 1.0f);

        // Assert
        result.Should().BeTrue();
        raisedEvents.Should().ContainSingle()
            .Which.Should().Be(new KeyValuePair<(InputDeviceEventType, string), float>((InputDeviceEventType.Axis, "Y"), 1.0f));
    }
}
