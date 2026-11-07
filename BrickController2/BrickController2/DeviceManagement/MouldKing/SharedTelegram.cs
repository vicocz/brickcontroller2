namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Wrapper of a telegram payload shared by multiple device instances
/// </summary>
internal sealed class SharedTelegram(byte[] payload)
{
    public byte[] Payload => payload;
}
