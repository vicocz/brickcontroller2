using Windows.Gaming.Input;

namespace BrickController2.Windows.Extensions;

public static class ControllerExtensions
{
    // deviceId looks like "{wgi/nrid/]Xd\\h-M1mO]-il0l-4L\\-Gebf:^3->kBRhM-d4}\0"
    // A unique ID that identifies the controller. As long as the controller is connected, the ID will never change.
    // JK: RawGameController.FromGameController(gamepad) returns null on deleted gamepad
    public static string? GetUniquePersistentDeviceId(this Gamepad gamepad) => RawGameController.FromGameController(gamepad)?.NonRoamableId;
}
