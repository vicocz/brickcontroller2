using System;
using System.Linq;

namespace BrickController2.Extensions;

public static class ArrayExtensions
{
    public static bool SequenceEqual<TItem>(this TItem[]? x, TItem[]? y)
    {
        if (x != null && y != null)
        {
            return x.AsSpan().SequenceEqual(y);
        }

        return x == null && y == null;
    }
}
