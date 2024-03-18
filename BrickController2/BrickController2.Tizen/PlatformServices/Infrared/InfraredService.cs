using BrickController2.PlatformServices.Infrared;
using Tizen.System;

namespace BrickController2.Tizen.PlatformServices.Infrared;

public class InfraredService : IInfraredService
{
    public bool IsInfraredSupported => IR.IsAvailable;

    public bool IsCarrierFrequencySupported(int carrierFrequency)
    {
        var frequencyRanges = _irManager.GetCarrierFrequencies();
        foreach (var range in frequencyRanges)
        {
            if (range.MinFrequency <= carrierFrequency && carrierFrequency <= range.MaxFrequency)
            {
                return true;
            }
        }

        return false;
    }

    public Task SendPacketAsync(int carrierFrequency, int[] packet)
    {
        IR.Transmit(carrierFrequency, packet);

        return Task.CompletedTask;
    }
}