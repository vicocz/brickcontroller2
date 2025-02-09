using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BrickController2.Extensions;

public static class ObservableCollectionExtensions
{
    public static bool Remove<T>(this ObservableCollection<T> collection, Func<T, bool> predicate, [MaybeNullWhen(false)] out T item)
        where T: class
    {
        if (collection != null)
        {
            var x = collection.Select((item, idx) => (item, idx)).FirstOrDefault(x => predicate(x.item));

            if (x.item != null)
            {
                collection.RemoveAt(x.idx);
                item = x.item;
                return true;
            }
        }

        item = default;
        return false;
    }
}
