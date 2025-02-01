using System;

namespace BrickController2.PlatformServices.GameController;

public static class GameController
{
    public const float BUTTON_DOWN = 1.0f;
    public const float BUTTON_UP = 0.0f;

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

    public static float AdjustControllerValue(float value)
    {
        value = Math.Abs(value) < 0.05 ? 0.0F : value;
        value = value > 0.95 ? 1.0F : value;
        value = value < -0.95 ? -1.0F : value;
        return value;
    }

    public static bool AreAlmostEqual(float a, float b) => Math.Abs(a - b) < 0.001;
}
