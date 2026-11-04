using System.Collections.Generic;
using System.Linq;

namespace BrickController2.DeviceManagement.Macros;

public sealed record MacroDescriptor
{
    public MacroDescriptor(
        string id,
        string nameKey,
        MacroScope scope,
        MacroKind kind,
        IReadOnlyList<MacroChoice>? choices = null,
        IReadOnlyList<int>? channels = null)
    {
        Channels = channels;
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

    /// <summary>
    /// Optional channels a channel macro can be assigned to; <c>null</c> means any channel.
    /// </summary>
    public IReadOnlyList<int>? Channels { get; }

    public bool SupportsChannel(int channel) => Channels is null || Channels.Contains(channel);

    public bool IsMatch(string macroId, MacroScope macroScope, MacroChoiceValue value, int? channel = null) => Id == macroId
        && Scope == macroScope
        && (channel is null || SupportsChannel(channel.Value))
        && (Choices.Count == 0 || (value.HasValue && Contains(value)));

    private bool Contains(MacroChoiceValue value)
    {
        foreach (var choice in Choices)
        {
            if (choice.Value.ValueEquals(value))
            {
                return true;
            }
        }
        return false;
    }
}
