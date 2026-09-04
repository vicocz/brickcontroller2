namespace BrickController2.DeviceManagement.FxBricks;

internal readonly record struct PfxFileDirEntry(
    ushort FileId,
    uint FileSize,
    ushort FirstSector,
    ushort Attributes,
    uint UserData1,
    uint UserData2,
    uint Crc32,
    string FileName);
