
namespace BrickController2.Helpers
{
    public static class GameControllerHelper
    {
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
    }
}
