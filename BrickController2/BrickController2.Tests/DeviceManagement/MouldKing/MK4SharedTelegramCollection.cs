using Xunit;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

/// <summary>
/// MK4 instances share a static telegram, so all tests creating MK4 devices must not run in parallel.
/// </summary>
[CollectionDefinition(DisableParallelization = true)]
public sealed class MK4SharedTelegramCollection;
