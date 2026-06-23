using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.PowerFunctions
{
    internal class PowerFunctionsDevice : Device, IDeviceType<PowerFunctionsDevice>
    {
        private readonly IPowerFunctionsManager _powerFunctionsManager;

        public PowerFunctionsDevice(string name, string address, byte[] deviceData, IPowerFunctionsManager powerFunctionsManager, IDeviceRepository deviceRepository)
            : base(name, address, deviceRepository)
        {
            _powerFunctionsManager = powerFunctionsManager;
        }

        public static DeviceType Type => DeviceType.Infrared;

        public static string TypeName => "Power Functions";
        public override DeviceType DeviceType => Type;
        public override int NumberOfChannels => 2;

        public override async Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            DeviceState = DeviceState.Connecting;

            var result = await _powerFunctionsManager.ConnectDevice(this);

            DeviceState = result == DeviceConnectionResult.Ok ? DeviceState.Connected : DeviceState.Disconnected;
            return result;
        }

        public override async Task DisconnectAsync()
        {
            DeviceState = DeviceState.Disconnecting;

            await _powerFunctionsManager.DisconnectDevice(this);

            DeviceState = DeviceState.Disconnected;
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(7 * value);
            _powerFunctionsManager.SetOutput(this, channel, intValue);
        }
    }
}
