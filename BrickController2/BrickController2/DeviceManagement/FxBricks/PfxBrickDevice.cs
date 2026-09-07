using BrickController2.DeviceManagement.IO;
using BrickController2.DeviceManagement.Macros;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.FxBricks;

internal class PfxBrickDevice : BluetoothDevice
{
    private const int PF_CHANNELS = 2;
    private const int LIGHT_CHANNELS = 8;
    private const string PlaySoundMacroId = "PlaySound";
    private const string StopSoundMacroId = "StopSound";
    private const string SetVolumeMacroId = "SetVolume";
    private const string IncreaseVolumeMacroId = "IncreaseVolume";
    private const string DecreaseVolumeMacroId = "DecreaseVolume";
    private const string PlaySoundMacroNameKey = "PfxPlaySoundMacro";
    private const string StopSoundMacroNameKey = "PfxStopSoundMacro";
    private const string SetVolumeMacroNameKey = "PfxSetVolumeMacro";
    private const string IncreaseVolumeMacroNameKey = "PfxIncreaseVolumeMacro";
    private const string DecreaseVolumeMacroNameKey = "PfxDecreaseVolumeMacro";

    private static readonly Guid SERVICE_UUID = new("49535343-fe7d-4ae5-8fa9-9fafd205e455");
    private static readonly Guid CHARACTERISTIC_UUID_WRITE = new("49535343-8841-43f4-a8d4-ecbe34729bb3");
    private static readonly Guid CHARACTERISTIC_UUID_NOTIFY = new("49535343-1e4d-4bd9-ba61-23c647249616");

    private static readonly IReadOnlyCollection<float> DefaultVolumes = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
    private static readonly IReadOnlyCollection<MacroDescriptor> StaticMacros =
    [
        new MacroDescriptor(id: SetVolumeMacroId,
            nameKey: SetVolumeMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot,
            choices: [.. DefaultVolumes.Select(x => MacroChoice.Create(x))]),
        new MacroDescriptor(id: IncreaseVolumeMacroId,
            nameKey: IncreaseVolumeMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot),
        new MacroDescriptor(id: DecreaseVolumeMacroId,
            nameKey: DecreaseVolumeMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot),
    ];

    private readonly OutputValuesGroup<short> _motorOutputs = new(PF_CHANNELS);
    private readonly OutputValuesGroup<short> _lightOutputs = new(LIGHT_CHANNELS);
    private readonly List<MacroDescriptor> _macros = [];
    private readonly Dictionary<string, byte> _macroFileIds = []; // MacroChoice<string>.Value (file id as string) -> PFx File ID

    private IGattCharacteristic? _writeCharacteristic;
    private IGattCharacteristic? _notifyCharacteristic;

    private TaskCompletionSource<byte[]>? _fileDirTcs;

    public PfxBrickDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
    }

    public override DeviceType DeviceType => DeviceType.PfxBrick;

    public override int NumberOfChannels => 10;

    public override bool SupportsMacros => true;

    public override IReadOnlyList<MacroDescriptor> AvailableMacros => _macros;

    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutput(int channel, float value)
    {
        CheckChannel(channel);
        value = CutOutputValue(value);

        if (channel >= PF_CHANNELS)
        {
            // Per light channel range: +- [0 .. 255]
            var brightnessValue = (short)(value * 255);
            int lightChannel = channel - PF_CHANNELS;
            _lightOutputs.SetOutput(lightChannel, brightnessValue);
        }
        else
        {
            // Per motor channel range: +- percent
            var percentValue = (short)(value * 100);
            _motorOutputs.SetOutput(channel, percentValue);
        }
    }

    public override Task<bool> ExecuteMacroAsync(MacroInvocation invocation, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (invocation.DescriptorId == PlaySoundMacroId
            && invocation.ChoiceValue is string fileName
            && _macroFileIds.TryGetValue(fileName, out var fileId))
        {
            return WriteCommandAsync(PfxProtocol.PlaySoundFile(fileId), token);
        }
        else if (invocation.DescriptorId == StopSoundMacroId
            && invocation.ChoiceValue is string stopFileName
            && _macroFileIds.TryGetValue(stopFileName, out var stopFileId))
        {
            return WriteCommandAsync(PfxProtocol.StopSoundFile(stopFileId), token);
        }
        else if (invocation.DescriptorId == SetVolumeMacroId
            && invocation.ChoiceValue is float volume)
        {
            return WriteCommandAsync(PfxProtocol.SetVolume((byte)volume), token);
        }
        else if (invocation.DescriptorId == IncreaseVolumeMacroId)
        {
            return WriteCommandAsync(PfxProtocol.IncreaseVolume(), token);
        }
        else if (invocation.DescriptorId == DecreaseVolumeMacroId)
        {
            return WriteCommandAsync(PfxProtocol.DecreaseVolume(), token);
        }

        // unknown command
        return Task.FromResult(false);
    }

    protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
        _writeCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_WRITE);

        _notifyCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_NOTIFY);
        if (_notifyCharacteristic is not null)
        {
            await _bleDevice!.EnableNotificationAsync(_notifyCharacteristic, token);
        }

        return _writeCharacteristic is not null;
    }

    protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
    {
        if (characteristicGuid != _notifyCharacteristic?.Uuid || data.Length == 0)
            return;

        if (data.Length == 1) // notification
        {
        }
        else if (data[0] == PfxProtocol.RSP_FILE_DIR)
        {
            _fileDirTcs?.TrySetResult(data);
        }
        else if (data.Length == 48) // status
        {
            HardwareVersion = $"{data[7]:X2}{data[8]:X2}"; // product_id
            FirmwareVersion = $"{data[37]:x2}.{data[38]:x2}"; // firmware_ver
        }
    }

    protected override async ValueTask BeforeDisconnectAsync(CancellationToken token)
    {
        if (_notifyCharacteristic != null && _bleDevice != null)
        {
            await _bleDevice.DisableNotificationAsync(_notifyCharacteristic, token);
        }
    }

    protected override void BeforeDisconnectCleanup()
    {
        _writeCharacteristic = null;
        _notifyCharacteristic = null;
        _macroFileIds.Clear();
    }

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        try
        {
            if (requestDeviceInformation)
            {
                await ReadDeviceInfo(token);
                await GetAvailableMacros(token);
            }
        }
        catch { }

        return true;
    }

    protected override async Task ProcessOutputsAsync(CancellationToken token)
    {
        try
        {
            // reset outputs
            _motorOutputs.Initialize();
            _lightOutputs.Initialize();

            while (!token.IsCancellationRequested)
            {
                bool changed = false;
                // process motor outputs for a change
                if (_motorOutputs.TryGetChanges(out var motorChanges))
                {
                    if (await SendOutputValuesAsync(motorChanges, token).ConfigureAwait(false))
                    {
                        // confirm successfull sending
                        _motorOutputs.Commit();
                        await Task.Delay(5, token).ConfigureAwait(false);
                    }
                    changed = true;
                }

                // process light outputs for a change
                if (_lightOutputs.TryGetChanges(out var lightChanges))
                {
                    if (await SendLightValuesAsync(lightChanges, token).ConfigureAwait(false))
                    {
                        // confirm successfull sending
                        _lightOutputs.Commit();
                        await Task.Delay(5, token).ConfigureAwait(false);
                    }
                    changed = true;
                }

                if (!changed)
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }

            // ensure everything is stopped in the end
            await WriteCommandAsync(PfxProtocol.AllOff(), token).ConfigureAwait(false);
        }
        catch
        {
        }
    }

    private async Task<bool> SendOutputValuesAsync(IEnumerable<KeyValuePair<int, short>> changes, CancellationToken token)
    {
        bool result = true;
        foreach (var change in changes)
        {
            var cmd = PfxProtocol.SetMotorSpeed(change.Key, change.Value);
            result &= await WriteCommandAsync(cmd, token);
        }
        return result;
    }

    private async Task<bool> SendLightValuesAsync(IEnumerable<KeyValuePair<int, short>> changes, CancellationToken token)
    {            
        bool result = true;
        foreach (var change in changes)
        {
            // apply brightness (if needed)
            if (change.Value != 0)
            {
                var cmd = PfxProtocol.SetBrightness(change.Key, change.Value);
                result &= await WriteCommandAsync(cmd, token);
            }
            // apply toggle ON / OFF based on value
            var toggleCmd = PfxProtocol.SetLight(change.Key, change.Value);
            result &= await WriteCommandAsync(toggleCmd, token);
        }
        return result;
    }

    private async Task<bool> WriteCommandAsync(byte[] command, CancellationToken token)
    {
        try
        {
            return await _bleDevice!.WriteAsync(_writeCharacteristic!, command, token);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task ReadDeviceInfo(CancellationToken token)
    {
        // request status update
        await _bleDevice!.WriteAsync(_writeCharacteristic!, PfxProtocol.GetStatus(), token);
    }

    private async Task GetAvailableMacros(CancellationToken token)
    {
        const byte MaxDirectorySlots = 64; // reference implementation caps the directory scan at 64 slots
        const byte FirstDirectoryIndex = 1; // directory index 0 is never a valid file slot

        _macros.Clear();
        _macros.AddRange(StaticMacros);
        _macroFileIds.Clear();

        var countResponse = await RequestFileDirAsync(PfxProtocol.GetFileCount(), token);
        var filesCount = PfxProtocol.ParseFileCount(countResponse) ?? 0;

        var foundCount = 0;
        var audioFilesChoices = new List<MacroChoice<string>>();

        for (byte i = FirstDirectoryIndex; i <= MaxDirectorySlots && foundCount < filesCount; i++)
        {
            var entryResponse = await RequestFileDirAsync(PfxProtocol.GetDirEntryAtIndex(i), token);
            var entry = PfxProtocol.ParseFileDirEntry(entryResponse);
            if (entry is null || !entry.Value.IsValid)
            {
                continue; // empty/unused directory slot
            }

            foundCount++;

            if (entry.Value.IsAudio)
            {
                audioFilesChoices.Add(new MacroChoice<string>(entry.Value.FileName, entry.Value.FileName));
                _macroFileIds[entry.Value.FileName] = (byte)entry.Value.FileId;
            }
        }

        _macros.Add(new MacroDescriptor(
            id: PlaySoundMacroId,
            nameKey: PlaySoundMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.Repeatable,
            choices: audioFilesChoices));

        _macros.Add(new MacroDescriptor(
            id: StopSoundMacroId,
            nameKey: StopSoundMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot,
            choices: audioFilesChoices));
    }

    private async Task<byte[]> RequestFileDirAsync(byte[] command, CancellationToken token, int timeoutMs = 2000)
    {
        _fileDirTcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        await WriteCommandAsync(command, token);

        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);
        using var reg = linkedCts.Token.Register(() => _fileDirTcs.TrySetCanceled(linkedCts.Token));

        return await _fileDirTcs.Task;
    }
}
