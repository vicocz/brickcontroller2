using System;

namespace BrickController2.PlatformServices.GameController;

public static class GameControllers
{
    public const float BUTTON_PRESSED = 1.0f;
    public const float BUTTON_RELEASED = 0.0f;

    public const float AXIS_DELTA_VALUE = 0.05f;

    public const float AXIS_ZERO_VALUE = 0.0f;
    public const float AXIS_MIN_VALUE = - 1.0f;
    public const float AXIS_MAX_VALUE = 1.0f;

    /// <summary>
    /// Creates an identifier string for the controller from the given index
    /// </summary>
    /// <param name="controllerIndex">zero-based index</param>
    /// <returns>Identifier</returns>
    public static string GetControllerIdFromIndex(int controllerIndex)
    {
        // controllerIndex == 0 -> "Controller 1"
        return $"Controller {controllerIndex + 1}";
    }

    /// <summary>
    /// Creates an identifier string for the controller from the given <paramref name="controllerNumber"/>
    /// </summary>
    /// <returns>Identifier</returns>
    public static string GetControllerIdFromNumber(int controllerNumber) =>
        // controllerIndex == 1 -> "Controller 1"
        $"Controller {controllerNumber}";

    public static float AdjustControllerValue(float value) => value switch
    {
        < -0.95f => AXIS_MIN_VALUE,
        > -0.05f and < 0.05f => AXIS_ZERO_VALUE,
        > 0.95f => AXIS_MAX_VALUE,
        _ => value
    };

    public static bool AreAlmostEqual(float a, float b) => Math.Abs(a - b) < 0.001;
}
