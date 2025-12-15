using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDeviceService;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BrickController2.DeviceManagement.Lego;

internal class LegoControllerService : InputDeviceServiceBase<LegoRemoteController>
{
    private readonly IDeviceManager _deviceManager;
    private readonly IInputDeviceEventServiceInternal _deviceEventServiceInternal;

    public LegoControllerService(IDeviceManager deviceManager,
        IInputDeviceManagerService inputDeviceManagerService,
        IInputDeviceEventServiceInternal deviceEventServiceInternal,
        ILogger<LegoControllerService> logger) 
        : base(inputDeviceManagerService, logger)
    {
        _deviceManager = deviceManager;
        _deviceEventServiceInternal = deviceEventServiceInternal;
    }

    public override void Initialize()
    {
        // process enabled only
        foreach (var remoteController in _deviceManager.Devices
            .OfType<RemoteControl>()
            .Where(c => c.IsEnabled))
        {
            var deviceNumber = GetFirstUnusedInputDeviceNumber();
            AddInputDevice(new LegoRemoteController(_deviceEventServiceInternal, remoteController, deviceNumber, _logger));
        }
    }

    public override void Stop()
    {
        while (TryRemoveInputDevice(x => true, out var controller))
        {
            _logger.LogDebug("Lego controller device has been removed InputDeviceId:{controllerId}", controller.InputDeviceId);
        }
    }
}
