using System;
using System.Linq;

namespace BrickController2.Extensions;

public static class ArrayExtensions
{
    /// <summary>
    /// This method is a concise, efficient, and null-safe way to compare two arrays of <typeparamref name="TItem"/> for equality,
    /// leveraging modern C# features like spans.
    /// </summary>
    public static bool SequenceEqual<TItem>(this TItem[]? x, TItem[]? y)
    {
        if (x != null && y != null)
        {
            return x.AsSpan().SequenceEqual(y);
        }

        return x == null && y == null;
    }
}
