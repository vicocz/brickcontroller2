using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace BrickController2.Extensions;

public static class CollectionExtensions
{
    public static int FindIndex<T>(this IList<T> collection, Predicate<T> match)
    {
        switch (collection)
        {
            case null:
                throw new ArgumentNullException(nameof(collection));

            case List<T> list:
                return list.FindIndex(match);

            default:
                for (int i = 0; i < collection.Count; i++)
                {
                    if (match(collection[i]))
                    {
                        return i;
                    }
                }
                return -1;
        }
    }

    public static bool Remove<T>(this IList<T> collection, Predicate<T> predicate, [MaybeNullWhen(false)] out T item)
        where T : class
    {
        var idx = collection.FindIndex(predicate);
        if (idx >= 0)
        {
            item = collection[idx];
            collection.RemoveAt(idx);
            return true;
        }

        item = default;
        return false;
    }
}
