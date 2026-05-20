using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.Lego;

internal class LegoRemoteController : InputDeviceBase<RemoteControl>
{
    private readonly ILogger _logger;

    public LegoRemoteController(IInputDeviceEventServiceInternal service,
        RemoteControl remoteControl,
        int controllerNumber,
        ILogger logger)
        : base(service, remoteControl)
    {
        Name = remoteControl.Name;
        InputDeviceNumber = controllerNumber;
        InputDeviceId = $"Controller ({remoteControl.Address})";
        _logger = logger;
    }

    public override void Start()
    {
        base.Start();
        // link Lego RemoteControl and connect
        InputDeviceDevice.ConnectInputController(this);
        // trigger device connection, but do not wait here
        _ = ConnectInputDeviceAsync();
    }

    public override void Stop()
    {
        base.Stop();
        // trigger device disconnection, but do not wait here
        _ = InputDeviceDevice.DisconnectAsync().ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                _logger.LogError(t.Exception, "Failed to disconnect Lego Remote Controller {inputDeviceId}", InputDeviceId);
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
        // reset Lego RemoteControl link
        InputDeviceDevice.DisconnectInputController();
    }

    private async Task ConnectInputDeviceAsync(CancellationToken token = default)
    {
        try
        {
            await InputDeviceDevice.ConnectAsync(false,
                (d) =>
                {
                    // reset events on random disconnection
                    InputDeviceDevice?.ResetEvents();
                },
                channelConfigurations: [],
                startOutputProcessing: false,
                requestDeviceInformation: false,
                token: token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect Lego Remote Controller {inputDeviceId}", InputDeviceId);
        }
    }
}
