namespace BrickController2.Protocols;

/// <summary>
/// static class wich implements the encryption algorithm for the advertising data
/// </summary>
public static class MKProtocol
{
    /// <summary>
    /// ManufacturerID for MK
    /// </summary>
    public const ushort ManufacturerID = 0xFFF0;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue1 = 0x3f;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue2 = 0x25;

    /// <summary>
    /// Address array
    /// </summary>
    public static readonly byte[] SeedArray = { 0xC1, 0xC2, 0xC3, 0xC4, 0xC5 };
}
