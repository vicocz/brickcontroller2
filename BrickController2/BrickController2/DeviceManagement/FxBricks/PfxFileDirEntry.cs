namespace BrickController2.DeviceManagement.FxBricks;

/// <summary>
/// File format identifier stored in the upper byte of the User Attributes field (ICD 9.1.5).
/// </summary>
internal enum PfxFileFormat : byte
{
    Wav = 0x00,
    Flac = 0x01,
    Mp3 = 0x02,
    Ogg = 0x03,
    Au = 0x04,
    Gsm = 0x05,
    Txt = 0x10,
    Hex = 0x11,
    Zip = 0x20,
    Gz = 0x21,
    Pfx = 0x30,
    Img = 0x50,
}

internal readonly record struct PfxFileDirEntry(
    ushort FileId,
    uint FileSize,
    ushort FirstSector,
    ushort Attributes,
    uint UserData1,
    uint UserData2,
    uint Crc32,
    string FileName)
{
    /// <summary>
    /// File format code stored in User Attributes[15:8] (ICD 9.1.5).
    /// </summary>
    public PfxFileFormat FileFormat => (PfxFileFormat)(Attributes >> 8);
}
