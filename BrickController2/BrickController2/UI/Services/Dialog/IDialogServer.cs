using BrickController2.PlatformServices.InputDeviceService;

namespace BrickController2.UI.Services.Dialog
{
    public interface IDialogServer : IDialogService
    {
        IInputDeviceEventService? InputDeviceEventService { get; set; }
    }
}
