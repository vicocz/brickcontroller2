using BrickController2.DeviceManagement.IO;
using BrickController2.DeviceManagement.Macros;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.FxBricks;

internal class PfxBrickDevice : BluetoothMacroBasedDevice
{
    private const int PF_CHANNELS = 2;
    private const int LIGHT_CHANNELS = 8;
    private const string PlaySoundMacroId = "PlaySound";
    private const string StopSoundMacroId = "StopSound";
    private const string ToggleSoundMacroId = "ToggleSound";
    private const string SetVolumeMacroId = "SetVolume";
    private const string IncreaseVolumeMacroId = "IncreaseVolume";
    private const string DecreaseVolumeMacroId = "DecreaseVolume";
    private const string RunScriptMacroId = "RunScript";
    private const string StopScriptMacroId = "StopScript";
    private const string RunScriptMacroNameKey = "PfxRunScriptMacro";
    private const string StopScriptMacroNameKey = "PfxStopScriptMacro";
    private const string PlaySoundMacroNameKey = "PfxPlaySoundMacro";
    private const string StopSoundMacroNameKey = "PfxStopSoundMacro";
    private const string ToggleSoundMacroNameKey = "PfxToggleSoundMacro";
    private const string SetVolumeMacroNameKey = "PfxSetVolumeMacro";
    private const string IncreaseVolumeMacroNameKey = "PfxIncreaseVolumeMacro";
    private const string DecreaseVolumeMacroNameKey = "PfxDecreaseVolumeMacro";

    private const string DefaultVolumeLevelName = "PfxBrickDefaultVolumeLevel";
    private static readonly Percent DefaultVolumeLevelValue = 50f;

    private static readonly Guid SERVICE_UUID = new("49535343-fe7d-4ae5-8fa9-9fafd205e455");
    private static readonly Guid CHARACTERISTIC_UUID_WRITE = new("49535343-8841-43f4-a8d4-ecbe34729bb3");
    private static readonly Guid CHARACTERISTIC_UUID_NOTIFY = new("49535343-1e4d-4bd9-ba61-23c647249616");

    private static readonly IReadOnlyCollection<float> DefaultVolumes = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
    private static readonly IReadOnlyCollection<float> LightEffectBrightnessLevels = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];

    private const string IncreaseBrightnessMacroId = "IncreaseBrightness";
    private const string DecreaseBrightnessMacroId = "DecreaseBrightness";
    private const string IncreaseBrightnessMacroNameKey = "PfxIncreaseBrightnessMacro";
    private const string DecreaseBrightnessMacroNameKey = "PfxDecreaseBrightnessMacro";

    private readonly record struct LightEffect(string NameKey, byte LightFxId, byte Param1 = 0x00, byte Param2 = 0x00, byte Param3 = 0x00);

    private static readonly IReadOnlyDictionary<string, LightEffect> LightEffectMacros = new Dictionary<string, LightEffect>
    {
        ["LightFlash50Pos"] = new("PfxLightFlash50PosMacro", PfxProtocol.EVT_LIGHTFX_FLASH_50_POS, 0x04, 0x03),
        ["LightFlash50Neg"] = new("PfxLightFlash50NegMacro", PfxProtocol.EVT_LIGHTFX_FLASH_50_NEG, 0x04, 0x03),
        ["LightStrobePos"] = new("PfxLightStrobePosMacro", PfxProtocol.EVT_LIGHTFX_STROBE_POS, 0x04, 0x04, 0x01),
        ["LightStrobeNeg"] = new("PfxLightStrobeNegMacro", PfxProtocol.EVT_LIGHTFX_STROBE_NEG, 0x04, 0x04, 0x01),
        ["LightGyralitePos"] = new("PfxLightGyralitePosMacro", PfxProtocol.EVT_LIGHTFX_GYRALITE_POS, 0x04, 0x09),
        ["LightGyraliteNeg"] = new("PfxLightGyraliteNegMacro", PfxProtocol.EVT_LIGHTFX_GYRALITE_NEG, 0x04, 0x09),
        ["LightFlicker"] = new("PfxLightFlickerMacro", PfxProtocol.EVT_LIGHTFX_FLICKER, 0x01, 0x06),
        ["LightRandomBlink"] = new("PfxLightRandomBlinkMacro", PfxProtocol.EVT_LIGHTFX_RANDOM_BLINK, 0x02, 0x03),
        ["LightPhotonTorpedo"] = new("PfxLightPhotonTorpedoMacro", PfxProtocol.EVT_LIGHTFX_PHOTON_TORPEDO, 0x0A),
        ["LightLaserPulse"] = new("PfxLightLaserPulseMacro", PfxProtocol.EVT_LIGHTFX_LASER_PULSE, 0x01),
        ["LightScifiEngineGlow"] = new("PfxLightScifiEngineGlowMacro", PfxProtocol.EVT_LIGHTFX_SCIFI_ENGINE_GLOW, 0x08, 0x09),
        ["LightLighthouse"] = new("PfxLightLighthouseMacro", PfxProtocol.EVT_LIGHTFX_LIGHTHOUSE, 0x0A),
        ["LightBrokenLight"] = new("PfxLightBrokenLightMacro", PfxProtocol.EVT_LIGHTFX_BROKEN_LIGHT, 0x02, 0x00, 0x02),
        ["LightSoundModulated"] = new("PfxLightSoundModulatedMacro", PfxProtocol.EVT_LIGHTFX_SOUND_MODULATED, 0x03),
    };
    private static readonly IReadOnlyList<int> LightMacroChannels = [.. Enumerable.Range(PF_CHANNELS, LIGHT_CHANNELS)];
    private static readonly IReadOnlyList<MacroDescriptor> DefaultStaticMacros =
    [
        new MacroDescriptor(id: SetVolumeMacroId,
            nameKey: SetVolumeMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot,
            choices: [.. DefaultVolumes.Select(MacroChoice.Create)]),
        new MacroDescriptor(id: IncreaseVolumeMacroId,
            nameKey: IncreaseVolumeMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot),
        new MacroDescriptor(id: DecreaseVolumeMacroId,
            nameKey: DecreaseVolumeMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot),
        new MacroDescriptor(
            id: StopScriptMacroId,
            nameKey: StopScriptMacroNameKey,
            scope: MacroScope.Device,
            kind: MacroKind.OneShot),
        new MacroDescriptor(
            id: IncreaseBrightnessMacroId,
            nameKey: IncreaseBrightnessMacroNameKey,
            scope: MacroScope.Channel,
            kind: MacroKind.OneShot,
            channels: LightMacroChannels),
        new MacroDescriptor(
            id: DecreaseBrightnessMacroId,
            nameKey: DecreaseBrightnessMacroNameKey,
            scope: MacroScope.Channel,
            kind: MacroKind.OneShot,
            channels: LightMacroChannels),
        .. LightEffectMacros.Select(m => new MacroDescriptor(
            id: m.Key,
            nameKey: m.Value.NameKey,
            scope: MacroScope.Channel,
            kind: MacroKind.OneShot,
            channels: LightMacroChannels,
            choices: [.. LightEffectBrightnessLevels.Select(MacroChoice.Create)])),
    ];

    private readonly OutputValuesGroup<short> _motorOutputs;
    private readonly OutputValuesGroup<short> _lightOutputs;
    private readonly ConcurrentDictionary<string, byte> _macroFileIds = [];
    private readonly SemaphoreSlim _fileDirSemaphore = new(1, 1);

    private IGattCharacteristic? _writeCharacteristic;
    private IGattCharacteristic? _notifyCharacteristic;

    private TaskCompletionSource<byte[]>? _fileDirTcs;

    public PfxBrickDevice(string name, string address, IEnumerable<NamedSetting> settings, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
        _motorOutputs = new(PF_CHANNELS, _outputLock);    // create with lock to ensure thread safety and data consistency when updating output values
        _lightOutputs = new(LIGHT_CHANNELS, _outputLock); // create with lock to ensure thread safety and data consistency when updating output values

        // apply values (if any) or default
        SetSettingValue(DefaultVolumeLevelName, settings, DefaultVolumeLevelValue);
    }

    public override DeviceType DeviceType => DeviceType.PfxBrick;

    public override int NumberOfChannels => 10;

    public Percent DefaultVolumeLevel => GetSettingValue(DefaultVolumeLevelName, DefaultVolumeLevelValue);

    protected override IReadOnlyList<MacroDescriptor> StaticMacros => DefaultStaticMacros;

    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutputs(IEnumerable<(int, float)> outputs)
    {
        lock (_outputLock)
        {
            foreach (var (channel, value) in outputs)
            {
                CheckChannel(channel);
                float setValue = CutOutputValue(value);

                if (channel >= PF_CHANNELS)
                {
                    // Per light channel range: +- [0 .. 255]
                    var brightnessValue = (short)(setValue * 255);
                    int lightChannel = channel - PF_CHANNELS;
                    _lightOutputs.SetOutput(lightChannel, brightnessValue);
                }
                else
                {
                    // Per motor channel range: +- percent
                    var percentValue = (short)(setValue * 100);
                    _motorOutputs.SetOutput(channel, percentValue);
                }
            }
        }
    }


    public override Task<bool> ExecuteMacroAsync(MacroInvocation invocation, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (DeviceState != DeviceState.Connected || _writeCharacteristic is null || _bleDevice is null)
        {
            return Task.FromResult(false);
        }

        if (invocation.DescriptorId == PlaySoundMacroId)
        {
            return EnqueueFileCommandAsync(invocation, id => PfxProtocol.PlaySoundFile(id), token);
        }
        else if (invocation.DescriptorId == StopSoundMacroId)
        {
            return EnqueueFileCommandAsync(invocation, id => PfxProtocol.StopSoundFile(id), token);
        }
        else if (invocation.DescriptorId == ToggleSoundMacroId)
        {
            return EnqueueFileCommandAsync(invocation, id => PfxProtocol.PlaySoundFile(id, PfxProtocol.EVT_SOUNDFX_RETRIGGER_TOGGLE), token);
        }
        else if (invocation.DescriptorId == RunScriptMacroId)
        {
            return EnqueueFileCommandAsync(invocation, id => PfxProtocol.RunScript(id), token);
        }
        else if (invocation.DescriptorId == StopScriptMacroId)
        {
            return EnqueueMacroCommandAsync(PfxProtocol.StopScript(), token);
        }
        else if (invocation.DescriptorId == SetVolumeMacroId
            && invocation.ChoiceValue.TryGet<float>(out var volume))
        {
            return EnqueueMacroCommandAsync(PfxProtocol.SetVolume((byte)volume), token);
        }
        else if (invocation.DescriptorId == IncreaseVolumeMacroId)
        {
            return EnqueueMacroCommandAsync(PfxProtocol.IncreaseVolume(), token);
        }
        else if (invocation.DescriptorId == DecreaseVolumeMacroId)
        {
            return EnqueueMacroCommandAsync(PfxProtocol.DecreaseVolume(), token);
        }
        else if (LightEffectMacros.TryGetValue(invocation.DescriptorId, out var lightEffect))
        {
            // light effects are supported on light channels only
            if (!TryGetLightChannel(invocation, out var lightChannel))
            {
                return Task.FromResult(false);
            }

            var brightness = invocation.ChoiceValue.TryGet<float>(out var brightnessPercent)
                ? (byte)Math.Clamp(Math.Round(brightnessPercent * 255 / 100), 0, 255)
                : (byte)255;

            return ExecuteLightEffectAsync(lightChannel, lightEffect, brightness, token);
        }
        else if (invocation.DescriptorId is IncreaseBrightnessMacroId or DecreaseBrightnessMacroId)
        {
            if (!TryGetLightChannel(invocation, out var lightChannel))
            {
                return Task.FromResult(false);
            }

            var lightFxId = invocation.DescriptorId == IncreaseBrightnessMacroId
                ? PfxProtocol.EVT_LIGHTFX_INC_BRIGHTNESS
                : PfxProtocol.EVT_LIGHTFX_DEC_BRIGHTNESS;
            return EnqueueMacroCommandAsync(PfxProtocol.SetLightEffect(lightChannel, lightFxId, PfxProtocol.EVT_LIGHTFX_TRANSITION_ON), token);
        }

        // unknown command
        return Task.FromResult(false);
    }

    private bool TryGetLightChannel(MacroInvocation invocation, out int lightChannel)
    {
        if (invocation.Channel is int channel && channel >= PF_CHANNELS && channel < NumberOfChannels)
        {
            lightChannel = channel - PF_CHANNELS;
            return true;
        }

        lightChannel = -1;
        return false;
    }

    private Task<bool> ExecuteLightEffectAsync(int lightChannel, LightEffect lightEffect, byte brightness, CancellationToken token)
    {
        // effects run at the light's current brightness, so apply requested brightness first;
        // both writes are one queued operation so they are cancelled/drained together
        return EnqueueMacroCommandsAsync(
        [
            PfxProtocol.SetBrightness(lightChannel, brightness),
            PfxProtocol.SetLightEffect(lightChannel,
                lightEffect.LightFxId,
                PfxProtocol.EVT_LIGHTFX_TRANSITION_TOGGLE,
                lightEffect.Param1,
                lightEffect.Param2,
                lightEffect.Param3),
        ], token);
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
        try
        {
            if (_notifyCharacteristic != null && _bleDevice != null)
            {
                await _bleDevice.DisableNotificationAsync(_notifyCharacteristic, token);
            }
            // ensure everything is stopped in the end
            await WriteCommandAsync(PfxProtocol.AllOff(), token).ConfigureAwait(false);
        }
        catch
        {
        }
        finally
        {
            // fail any commands left queued when the loop stops (disconnect/cancel)
            await base.BeforeDisconnectAsync(token);
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
            }

            await WriteCommandAsync(PfxProtocol.SetVolume(DefaultVolumeLevel.Value), token).ConfigureAwait(false);
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
            _macroFileIds.Clear();

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
                // macro commands - take the first available
                if (await ProcessMacroCommandQueueAsync(token).ConfigureAwait(false))
                {
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

    protected override ValueTask<IReadOnlyList<MacroDescriptor>> DiscoverDynamicMacrosAsync(CancellationToken token)
        => GetAvailableMacrosAsync(token);

    protected override Task<bool> WriteMacroCommandAsync(byte[] command, CancellationToken token)
        => WriteCommandAsync(command, token);

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

    private async ValueTask<IReadOnlyList<MacroDescriptor>> GetAvailableMacrosAsync(CancellationToken token)
    {
        const byte MaxDirectorySlots = 64; // reference implementation caps the directory scan at 64 slots
        const byte FirstDirectoryIndex = 1; // directory index 0 is never a valid file slot

        var countResponse = await RequestFileDirAsync(PfxProtocol.GetFileCount(), token);
        var filesCount = PfxProtocol.ParseFileCount(countResponse) ?? 0;

        var foundCount = 0;
        var audioFilesChoices = new List<MacroChoice<string>>();
        var scriptFilesChoices = new List<MacroChoice<string>>();

        for (byte i = FirstDirectoryIndex; i <= MaxDirectorySlots && foundCount < filesCount && !token.IsCancellationRequested; i++)
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
            else if (entry.Value.IsScript)
            {
                scriptFilesChoices.Add(new MacroChoice<string>(entry.Value.FileName, entry.Value.FileName));
                _macroFileIds[entry.Value.FileName] = (byte)entry.Value.FileId;
            }
        }

        var macros = new List<MacroDescriptor>();

        if (audioFilesChoices.Count > 0)
        {
            macros.Add(new MacroDescriptor(
                id: PlaySoundMacroId,
                nameKey: PlaySoundMacroNameKey,
                scope: MacroScope.Device,
                kind: MacroKind.Repeatable,
                choices: audioFilesChoices));
            macros.Add(new MacroDescriptor(
                id: StopSoundMacroId,
                nameKey: StopSoundMacroNameKey,
                scope: MacroScope.Device,
                kind: MacroKind.OneShot,
                choices: audioFilesChoices));
            macros.Add(new MacroDescriptor(
                id: ToggleSoundMacroId,
                nameKey: ToggleSoundMacroNameKey,
                scope: MacroScope.Device,
                kind: MacroKind.OneShot,
                choices: audioFilesChoices));
        }

        if (scriptFilesChoices.Count > 0)
        {
            macros.Add(new MacroDescriptor(
                id: RunScriptMacroId,
                nameKey: RunScriptMacroNameKey,
                scope: MacroScope.Device,
                kind: MacroKind.OneShot,
                choices: scriptFilesChoices));
        }

        return macros;
    }

    private Task<bool> EnqueueFileCommandAsync(MacroInvocation invocation, Func<byte, byte[]> buildCommand, CancellationToken token)
    {
        if (!invocation.ChoiceValue.TryGet<string>(out var fileName))
        {
            return Task.FromResult(false);
        }

        return EnqueueMacroCommandAsync(async t =>
            {
                if (!_macroFileIds.TryGetValue(fileName, out var fileId))
                {
                    var resolvedFileId = await ResolveFileIdAsync(fileName, t).ConfigureAwait(false);
                    if (resolvedFileId is null)
                    {
                        return null; // unknown file, nothing to write
                    }

                    fileId = resolvedFileId.Value;
                    _macroFileIds[fileName] = fileId;
                }

                return buildCommand(fileId);
            }, token);
    }

    private async Task<byte?> ResolveFileIdAsync(string fileName, CancellationToken token)
    {
        var response = await RequestFileDirAsync(PfxProtocol.GetNamedFileId(fileName), token);
        return PfxProtocol.ParseNamedFileId(response);
    }

    private async Task<byte[]> RequestFileDirAsync(byte[] command, CancellationToken token, int timeoutMs = 2000)
    {
        await _fileDirSemaphore.WaitAsync(token);
        try
        {
            _fileDirTcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            await WriteCommandAsync(command, token);

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);
            using var reg = linkedCts.Token.Register(() => _fileDirTcs.TrySetCanceled(linkedCts.Token));

            return await _fileDirTcs.Task;
        }
        finally
        {
            _fileDirTcs = null; // Clean up
            _fileDirSemaphore.Release();
        }
    }
}
