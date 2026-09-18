using BrickController2.DeviceManagement.Macros;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Base class for Bluetooth devices that support dynamic macros
/// </summary>
internal abstract class BluetoothMacroBasedDevice : BluetoothDevice
{
    protected readonly AsyncLock _macroLock = new();
    private IReadOnlyList<MacroDescriptor> _availableMacros;

    private readonly ConcurrentQueue<QueuedMacroCommand> _macroCommandQueue = new();

    private readonly record struct QueuedMacroCommand(Func<CancellationToken, Task<byte[]?>> Resolve,
        TaskCompletionSource<bool> Completion,
        CancellationToken CallerToken)
    {
        public bool TrySetCanceled(CancellationToken token)
        {
            var actualCancelToken = CallerToken.IsCancellationRequested ? CallerToken : token;
            return Completion.TrySetCanceled(actualCancelToken);
        }

        public bool TrySetResult(bool result) => Completion.TrySetResult(result);
    }

    public BluetoothMacroBasedDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
        _availableMacros = StaticMacros;
    }

    public sealed override bool SupportsMacros => true;
    public sealed override bool SupportsDynamicMacros => true;
    public sealed override IReadOnlyList<MacroDescriptor> AvailableMacros => Volatile.Read(ref _availableMacros);

    protected virtual IReadOnlyList<MacroDescriptor> StaticMacros => [];

    public sealed override async ValueTask<IReadOnlyList<MacroDescriptor>> GetMacrosAsync(bool forceRefresh = false, CancellationToken token = default)
    {
        if (!forceRefresh || !SupportsDynamicMacros || DeviceState != DeviceState.Connected)
        {
            return AvailableMacros;
        }

        try
        {
            using (await _macroLock.LockAsync(token))
            {
                if (DeviceState != DeviceState.Connected)
                {
                    return AvailableMacros;
                }

                var discoveredMacros = await DiscoverDynamicMacrosAsync(token);
                UpdateMacroCache(discoveredMacros);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // caller-requested cancellation (e.g. navigating away/dialog cancel); return cached results
        }
        catch (OperationCanceledException ex)
        {
            // this is an internal timeout/cancellation inside DiscoverDynamicMacrosAsync;
            throw new MacroDiscoveryException("Dynamic macro discovery timed out.", ex);
        }

        return AvailableMacros;
    }

    protected override async ValueTask BeforeDisconnectAsync(CancellationToken token)
    {
        try
        {
            await base.BeforeDisconnectAsync(token);
        }
        finally
        {
            // fail any commands left queued when the loop stops (disconnect/cancel)
            DrainMacroCommandQueue();
        }
    }

    protected abstract ValueTask<IReadOnlyList<MacroDescriptor>> DiscoverDynamicMacrosAsync(CancellationToken token);

    protected void UpdateMacroCache(IReadOnlyList<MacroDescriptor>? dynamicMacros)
    {
        dynamicMacros ??= [];

        _availableMacros = [.. StaticMacros, .. dynamicMacros];
        RaisePropertyChanged(nameof(AvailableMacros));
    }

    /// <summary>
    /// Writes a resolved macro command to the device.
    /// </summary>
    protected abstract Task<bool> WriteMacroCommandAsync(byte[] command, CancellationToken token);

    /// <summary>
    /// Dequeues and processes a single queued macro command (if any).
    /// Intended to be called once per iteration of a subclass's output-processing loop.
    /// </summary>
    protected async Task<bool> ProcessMacroCommandQueueAsync(CancellationToken token)
    {
        if (_macroCommandQueue.TryDequeue(out var queued) &&
            !queued.Completion.Task.IsCompleted)
        {
            return await ProcessMacroCommandAsync(queued, token).ConfigureAwait(false);
        }

        return false;
    }

    protected Task<bool> EnqueueMacroCommandAsync(byte[] command, CancellationToken token)
        => EnqueueMacroCommandAsync(_ => Task.FromResult<byte[]?>(command), token);

    protected Task<bool> EnqueueMacroCommandAsync(Func<CancellationToken, Task<byte[]?>> resolve, CancellationToken token)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (token.IsCancellationRequested)
        {
            tcs.TrySetCanceled(token);
            return tcs.Task;
        }

        _macroCommandQueue.Enqueue(new QueuedMacroCommand(resolve, tcs, token));

        if (token.CanBeCanceled)
        {
            var registration = token.Register(static state =>
            {
                var (t, ct) = ((TaskCompletionSource<bool>, CancellationToken))state!;
                t.TrySetCanceled(ct);
            }, (tcs, token));
            tcs.Task.ContinueWith(
                _ => registration.Dispose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        return tcs.Task;
    }

    private async Task<bool> ProcessMacroCommandAsync(QueuedMacroCommand queued, CancellationToken token)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, queued.CallerToken);
        var linkedToken = linkedCts.Token;

        try
        {
            if (linkedToken.IsCancellationRequested)
            {
                queued.TrySetCanceled(token);
                return false;
            }

            var resolvedCommand = await queued.Resolve(linkedToken).ConfigureAwait(false);
            if (resolvedCommand is null)
            {
                queued.TrySetResult(false);
                return false; // resolution failed (e.g. unknown file), nothing to write
            }

            if (linkedToken.IsCancellationRequested)
            {
                // caller cancelled while we were resolving the file id; don't send the command
                queued.TrySetCanceled(token);
                return false;
            }

            var macroResult = await WriteMacroCommandAsync(resolvedCommand, linkedToken).ConfigureAwait(false);
            queued.TrySetResult(macroResult);
            return true;
        }
        catch (OperationCanceledException) when (linkedToken.IsCancellationRequested)
        {
            // either the loop's token was cancelled (e.g. disconnect) or the caller's token
            // was cancelled while resolving/writing this already-dequeued item; complete it here
            queued.TrySetCanceled(token);

            if (token.IsCancellationRequested)
            {
                throw;
            }

            return false;
        }
        catch
        {
            queued.TrySetResult(false);
            return false;
        }
    }

    private void DrainMacroCommandQueue()
    {
        while (_macroCommandQueue.TryDequeue(out var queued))
        {
            queued.Completion.TrySetCanceled();
        }
    }
}