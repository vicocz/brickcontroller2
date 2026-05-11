using BrickController2.DeviceManagement.IO;

namespace BrickController2.DeviceManagement.Lego;

internal class ChannelPositionStore : StateStore<int, PositionInfo>
{
    public ChannelPositionStore() : base(PositionInfo.Initial)
    {
    }

    internal PositionInfo ConsumeUpdate(int channel) => Exchange(channel, x => x.ConsumeUpdate());
}
