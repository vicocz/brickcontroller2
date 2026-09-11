using BrickController2.DeviceManagement.FxBricks;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.Protocols;

public class PfxProtocolTests
{
    [Fact]
    public void GetStatus_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.GetStatus();

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, // CMD_PRE_DELIMITER
            0x01,             // CMD_GET_STATUS
            0xA5, 0x5A, 0x6E, 0x40, 0x54, 0xA4, 0xE5, // Status bytes
            0x5D, 0x5D, 0x5D  // CMD_POST_DELIMITER
        });
    }

    [Fact]
    public void AllOff_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.AllOff();

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, // CMD_PRE_DELIMITER
            0x13,             // CMD_TEST_ACTION
            0x01,             // EVT_COMMAND_ALL_OFF
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x5D, 0x5D, 0x5D  // CMD_POST_DELIMITER
        });
    }

    [Theory]
    [InlineData(0, 50, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x71, 0x9F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData(1, -100, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x72, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData(int.MaxValue, 0, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x73, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    public void SetMotorSpeed_ShouldReturnExpectedByteArray(int channel, short speed, byte[] expected)
    {
        // Act
        var result = PfxProtocol.SetMotorSpeed(channel, speed);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, 128, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData(1, 255, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x00, 0x00, 0x00, 0x04, 0x02, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData(int.MaxValue, 255, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x00, 0x00, 0x00, 0x04, 0xFF, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    public void SetBrightness_ShouldReturnExpectedByteArray(int lightChannel, short value, byte[] expected)
    {
        // Act
        var result = PfxProtocol.SetBrightness(lightChannel, value);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, 128, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData(7, 0, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x00, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData(int.MaxValue, 255, new byte[] { 0x5B, 0x5B, 0x5B, 0x13, 0x00, 0x00, 0x00, 0x00, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x5D, 0x5D })]
    public void SetLight_ShouldReturnExpectedByteArray(int lightChannel, short value, byte[] expected)
    {
        // Act
        var result = PfxProtocol.SetLight(lightChannel, value);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetFileCount_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.GetFileCount();

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, // CMD_PRE_DELIMITER
            0x45,             // CMD_FILE_DIR
            0x00,             // PFX_DIR_REQ_GET_FILE_COUNT
            0x5D, 0x5D, 0x5D  // CMD_POST_DELIMITER
        });
    }

    [Theory]
    [InlineData((byte)0, new byte[] { 0x5B, 0x5B, 0x5B, 0x45, 0x02, 0x00, 0x5D, 0x5D, 0x5D })]
    [InlineData((byte)5, new byte[] { 0x5B, 0x5B, 0x5B, 0x45, 0x02, 0x05, 0x5D, 0x5D, 0x5D })]
    [InlineData((byte)255, new byte[] { 0x5B, 0x5B, 0x5B, 0x45, 0x02, 0xFF, 0x5D, 0x5D, 0x5D })]
    public void GetDirEntryAtIndex_ShouldReturnExpectedByteArray(byte index, byte[] expected)
    {
        var result = PfxProtocol.GetDirEntryAtIndex(index);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(new byte[] { 0xC5, 0x00, 0x00, 0x05 }, (ushort)5)]        // 4-byte ICD layout
    [InlineData(new byte[] { 0xC5, 0x00, 0x00, 0x00, 0x0A }, (ushort)10)] // 5-byte observed layout with echoed sub-code
    public void ParseFileCount_WithValidResponse_ShouldReturnExpectedCount(byte[] data, ushort expected)
    {
        var result = PfxProtocol.ParseFileCount(data);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(new byte[] { 0xC5, 0x00, 0x00 })]        // too short
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x05 })]  // wrong response header
    public void ParseFileCount_WithInvalidResponse_ShouldReturnNull(byte[] data)
    {
        var result = PfxProtocol.ParseFileCount(data);

        result.Should().BeNull();
    }

    [Fact]
    public void ParseFileDirEntry_WithValidResponse_ShouldReturnExpectedEntry()
    {
        var data = new byte[]
        {
            0xC5, 0x03,                     // RSP_FILE_DIR, echoed request sub-code
            0x00, 0x01,                     // FileId = 1
            0x00, 0x00, 0x00, 0x64,          // FileSize = 100
            0x00, 0x05,                     // FirstSector = 5
            0x02, 0x00,                     // Attributes = 0x0200 (FileFormat.Mp3 in high byte)
            0x11, 0x22, 0x33, 0x44,          // UserData1
            0x55, 0x66, 0x77, 0x88,          // UserData2
            0xAA, 0xBB, 0xCC, 0xDD,          // Crc32
            (byte)'t', (byte)'e', (byte)'s', (byte)'t', (byte)'.', (byte)'m', (byte)'p', (byte)'3' // FileName
        };

        var result = PfxProtocol.ParseFileDirEntry(data);

        result.Should().NotBeNull();
        result!.Value.FileId.Should().Be(1);
        result.Value.FileSize.Should().Be(100u);
        result.Value.Attributes.Should().Be(0x0200);
        result.Value.UserData1.Should().Be(0x11223344u);
        result.Value.UserData2.Should().Be(0x55667788u);
        result.Value.FileName.Should().Be("test.mp3");
        result.Value.FileFormat.Should().Be(PfxProtocol.FileFormat.Mp3);
        result.Value.IsValid.Should().BeTrue();
        result.Value.IsAudio.Should().BeTrue();
    }

    [Fact]
    public void ParseFileDirEntry_WithEmptySlot_ShouldReturnInvalidEntry()
    {
        var data = new byte[24];
        data[0] = 0xC5;
        data[8] = 0xFF;
        data[9] = 0xFF; // FirstSector = 0xFFFF marks an empty/unused directory slot

        var result = PfxProtocol.ParseFileDirEntry(data);

        result.Should().NotBeNull();
        result.Value.FileName.Should().BeEmpty();
        result.Value.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ParseFileDirEntry_WithTooShortResponse_ShouldReturnNull()
    {
        var data = new byte[] { 0xC5, 0x00, 0x00 };

        var result = PfxProtocol.ParseFileDirEntry(data);

        result.Should().BeNull();
    }

    [Fact]
    public void ParseFileDirEntry_WithWrongHeader_ShouldReturnNull()
    {
        var data = new byte[24];

        var result = PfxProtocol.ParseFileDirEntry(data);

        result.Should().BeNull();
    }

    [Fact]
    public void PlaySoundFile_WithDefaultArgs_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.PlaySoundFile(10);

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, 0x13,
            0x00,                                           // command
            0x00, 0x00, 0x00,                               // motor
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // light
            0x04,                                           // soundFxId = EVT_SOUNDFX_PLAY_ONCE
            0x0A,                                           // soundFileId = 10
            0x01,                                           // soundParam1 = EVT_SOUNDFX_RETRIGGER_RESTART
            0x00,                                           // soundParam2 = relativeVolume 0
            0x5D, 0x5D, 0x5D
        });
    }

    [Fact]
    public void PlaySoundFile_WithCustomArgs_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.PlaySoundFile(200, PfxProtocol.EVT_SOUNDFX_RETRIGGER_TOGGLE, -3);

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, 0x13,
            0x00,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x04,       // soundFxId = EVT_SOUNDFX_PLAY_ONCE
            0xC8,       // soundFileId = 200
            0x00,       // soundParam1 = EVT_SOUNDFX_RETRIGGER_TOGGLE
            0xFD,       // soundParam2 = -3 as unchecked byte
            0x5D, 0x5D, 0x5D
        });
    }

    [Fact]
    public void StopSoundFile_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.StopSoundFile(15);

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, 0x13,
            0x00,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0B, // soundFxId = EVT_SOUNDFX_STOP
            0x0F, // soundFileId = 15
            0x00,
            0x00,
            0x5D, 0x5D, 0x5D
        });
    }

    [Theory]
    [InlineData(-10f, (byte)0)]
    [InlineData(0f, (byte)0)]
    [InlineData(50f, (byte)127)]
    [InlineData(100f, (byte)255)]
    [InlineData(150f, (byte)255)]
    public void SetVolume_ShouldReturnExpectedByteArray(float volume, byte expectedSoundParam2)
    {
        var result = PfxProtocol.SetVolume(volume);

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, 0x13,
            0x00,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x03, // soundFxId = EVT_SOUNDFX_SET_VOLUME
            0x00,
            0x00,
            expectedSoundParam2,
            0x5D, 0x5D, 0x5D
        });
    }

    [Fact]
    public void IncreaseVolume_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.IncreaseVolume();

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, 0x13,
            0x00,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, // soundFxId = EVT_SOUNDFX_INC_VOLUME
            0x00, 0x00, 0x00,
            0x5D, 0x5D, 0x5D
        });
    }

    [Fact]
    public void DecreaseVolume_ShouldReturnExpectedByteArray()
    {
        var result = PfxProtocol.DecreaseVolume();

        result.Should().BeEquivalentTo(new byte[]
        {
            0x5B, 0x5B, 0x5B, 0x13,
            0x00,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, // soundFxId = EVT_SOUNDFX_DEC_VOLUME
            0x00, 0x00, 0x00,
            0x5D, 0x5D, 0x5D
        });
    }

    [Theory]
    [InlineData((ushort)0x0000, (byte)0x00, true)]
    [InlineData((ushort)0x0200, (byte)0x02, true)]
    [InlineData((ushort)0x0500, (byte)0x05, true)]
    [InlineData((ushort)0x1000, (byte)0x10, false)]
    [InlineData((ushort)0x3000, (byte)0x30, false)]
    public void FileDirEntry_FileFormatAndIsAudio_ShouldBeComputedFromAttributes(ushort attributes, byte expectedFormat, bool expectedIsAudio)
    {
        var entry = new PfxProtocol.FileDirEntry(1, 100, attributes, 0, 0, "file");

        ((byte)entry.FileFormat).Should().Be(expectedFormat);
        entry.IsAudio.Should().Be(expectedIsAudio);
    }

    [Theory]
    [InlineData((ushort)5, "file.mp3", true)]
    [InlineData((ushort)0xFFFF, "file.mp3", false)]
    [InlineData((ushort)5, "", false)]
    public void FileDirEntry_IsValid_ShouldReturnExpectedValue(ushort fileId, string fileName, bool expected)
    {
        var entry = new PfxProtocol.FileDirEntry(fileId, 100, 0x0500, 0, 0, fileName);

        entry.IsValid.Should().Be(expected);
    }
}
