using System;
using System.Diagnostics;

namespace BrickController2.Diagnostics;

internal static class Logs
{
    [Conditional("DEBUG")]
    public static void Dump(string label, ReadOnlySpan<byte> data)
    {
        var s = Convert.ToHexString(data);
        Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.f} {label}: {s}");
    }

    [Conditional("DEBUG")]
    public static void Dump<T>(string label, T data)
    {
        Debug.WriteLine($"{DateTimeOffset.Now:HH:mm:ss.f} {label}: [{data}]");
    }
}
