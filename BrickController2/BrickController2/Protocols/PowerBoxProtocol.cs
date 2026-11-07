namespace BrickController2.Protocols;

/// <summary>
/// static class which implements the encryption algorithm for the advertising data
/// </summary>
public static class PowerBoxProtocol
{
    /// <summary>
    /// ManufacturerID for PowerBox
    /// </summary>
    public const ushort ManufacturerID = 0xFF00;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue1 = 0x3f;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue2 = 0x25;

    /// <summary>
    /// SeedArray
    /// </summary>
    public static readonly byte[] SeedArray = 
    {
        0xC1, 
        0xC2, 
        0xC3, 
        0xC4, 
        0xC5, 
    };

    /// <summary>
    /// HeaderArray
    /// </summary>
    public static readonly byte[] HeaderArray =
    {
        0x71,   // 0x71 (113)
        0x0f,   // 0x0f (15)
        0x55,   // 0x55 (85)
    };
}
