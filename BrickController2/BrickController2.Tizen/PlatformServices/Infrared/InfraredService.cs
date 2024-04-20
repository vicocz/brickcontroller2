using BrickController2.PlatformServices.Infrared;

namespace BrickController2.Tizen.PlatformServices.Infrared;

public class InfraredService : IInfraredService
{
    public bool IsInfraredSupported => false;

    public bool IsCarrierFrequencySupported(int carrierFrequency) => throw new InvalidOperationException();

    public Task SendPacketAsync(int carrierFrequency, int[] packet) => throw new InvalidOperationException();
}