using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.Macros;
using Xunit;

namespace BrickController2.Tests.CreationManagement;

public class ControllerActionTests
{
    private const string MacroId = "macro-1";

    [Fact]
    public void IsValidMacro_ReturnsTrue_WhenDescriptorHasNoChoices_AndSavedChoiceIsEmpty()
    {
        var action = CreateAction(default);
        var macro = CreateMacro();

        Assert.True(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsTrue_WhenDescriptorHasNoChoices_AndSavedChoiceIsStale()
    {
        var action = CreateAction("deleted.mp3");
        var macro = CreateMacro();

        Assert.True(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsTrue_WhenSavedChoiceMatchesString()
    {
        var action = CreateAction("sound1.mp3");
        var macro = CreateMacro(
            new MacroChoice<string>("sound1", "sound1.mp3"),
            new MacroChoice<string>("sound2", "sound2.mp3"));

        Assert.True(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenSavedFileIsNoLongerAvailable()
    {
        var action = CreateAction("deleted.mp3");
        var macro = CreateMacro(
            new MacroChoice<string>("sound1", "sound1.mp3"),
            new MacroChoice<string>("sound2", "sound2.mp3"));

        Assert.False(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenDescriptorHasChoices_AndNoChoiceSaved()
    {
        var action = CreateAction(default);
        var macro = CreateMacro(new MacroChoice<string>("sound1", "sound1.mp3"));

        Assert.False(action.IsValidMacro(macro));
    }

    [Theory]
    [InlineData(50)]
    [InlineData(50L)]
    [InlineData(50.0)]
    [InlineData(50f)]
    public void IsValidMacro_ReturnsTrue_WhenSavedNumericChoiceHasDifferentBoxedType(object savedValue)
    {
        var action = CreateAction(new MacroChoiceValue(savedValue));
        var macro = CreateMacro(MacroChoice.Create(50f), MacroChoice.Create(60f));

        Assert.True(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenSavedNumericChoiceIsNotAvailable()
    {
        var action = CreateAction(new MacroChoiceValue(55L));
        var macro = CreateMacro(MacroChoice.Create(50f), MacroChoice.Create(60f));

        Assert.False(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenSavedStringIsComparedWithNumericChoice()
    {
        var action = CreateAction("50");
        var macro = CreateMacro(MacroChoice.Create(50f));

        Assert.False(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenMacroIdDiffers()
    {
        var action = CreateAction(default);
        var macro = new MacroDescriptor("other", "name", MacroScope.Device, MacroKind.OneShot);

        Assert.False(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenScopeDoesNotMatchChannel()
    {
        var action = CreateAction(default, channel: 1);
        var macro = CreateMacro();

        Assert.False(action.IsValidMacro(macro));
    }

    [Fact]
    public void IsValidMacro_ReturnsFalse_WhenButtonTypeIsNotMacro()
    {
        var action = CreateAction(default);
        action.ButtonType = ControllerButtonType.Normal;

        Assert.False(action.IsValidMacro(CreateMacro()));
    }

    private static ControllerAction CreateAction(MacroChoiceValue choice, int channel = ControllerAction.NoChannel)
        => new()
        {
            DeviceId = "device-1",
            ButtonType = ControllerButtonType.Macro,
            MacroId = MacroId,
            Channel = channel,
            MacroChoice = choice
        };

    private static MacroDescriptor CreateMacro(params MacroChoice[] choices)
        => new(MacroId, "name", MacroScope.Device, MacroKind.OneShot, choices);
}
