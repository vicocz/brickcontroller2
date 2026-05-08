namespace BrickController2.DeviceManagement.IO;

internal class ChannelStateStore<TValue> : StateStore<int, TValue>
    where TValue : struct
{
    public ChannelStateStore(TValue initialState = default) : base(initialState)
    {
    }
}
