using Android.Views;

namespace BrickController2.Droid.PlatformServices.GameController;

internal static class InputEventExtensions
{
    internal static bool IsGameControllerButtonEvent(this KeyEvent? keyEvent) => keyEvent != null &&
        keyEvent.Source.IsButtonEventSource() &&
        keyEvent.RepeatCount == 0;

    internal static bool IsButtonEventSource(this InputSourceType sourceType) => sourceType.HasFlag(InputSourceType.Gamepad);

    internal static bool IsGameControllerAxisEvent(this MotionEvent? motionEvent) => motionEvent != null &&
        motionEvent.Source.IsAxisEventSource() &&
        motionEvent.Action == MotionEventActions.Move;

    internal static bool IsAxisEventSource(this InputSourceType sourceType) => sourceType.HasFlag(InputSourceType.Joystick);
}
