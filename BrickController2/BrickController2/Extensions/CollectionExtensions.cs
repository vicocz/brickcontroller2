using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace BrickController2.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Searches index of an item that matches the specified predicate <paramref name="predicate"/>
    /// </summary>
    public static int FindIndex<T>(this IList<T> collection, Predicate<T> predicate)
    {
        switch (collection)
        {
            case null:
                throw new ArgumentNullException(nameof(collection));

            case List<T> list:
                return list.FindIndex(predicate);

            default:
                for (int i = 0; i < collection.Count; i++)
                {
                    if (predicate(collection[i]))
                    {
                        return i;
                    }
                }
                return -1;
        }
    }

    /// <summary>
    /// Remove the first item matching the prediccate specified in <paramref name="predicate"/>
    /// </summary>
    public static bool Remove<T>(this IList<T> collection, Predicate<T> predicate, [MaybeNullWhen(false)] out T item)
        where T : class
    {
        var idx = collection.FindIndex(predicate);
        if (idx < 0)
        {
            item = default;
            return false;
        }

        item = collection[idx];
        collection.RemoveAt(idx);
        return true;
    }
}
