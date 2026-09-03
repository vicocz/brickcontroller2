using System.Collections.Generic;

namespace BrickController2.DeviceManagement.Macros;

public sealed record MacroDescriptor
{
    public MacroDescriptor(
        string id,
        string nameKey,
        MacroScope scope,
        MacroKind kind,
        IReadOnlyList<MacroChoice>? choices = null)
    {
        Id = id;
        NameKey = nameKey;
        Scope = scope;
        Kind = kind;
        Choices = choices ?? [];
    }

    public string Id { get; }
    public string NameKey { get; }
    public MacroScope Scope { get; }
    public MacroKind Kind { get; }
    public IReadOnlyList<MacroChoice> Choices { get; }
}
