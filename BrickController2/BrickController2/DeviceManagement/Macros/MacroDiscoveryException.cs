using System;

namespace BrickController2.DeviceManagement.Macros;

/// <summary>
/// Thrown when dynamic macro discovery fails or times out for reasons other than
/// the caller-supplied CancellationToken being canceled.
/// </summary>
public sealed class MacroDiscoveryException : Exception
{
    public MacroDiscoveryException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
