using System;
using System.Linq;

namespace BrickController2.Helpers;

public static class StringHelper
{
    /// <summary>
    /// Create a alphanumeric random string
    /// </summary>
    /// <param name="length"></param>
    /// <returns>alphanumeric random string</returns>
    public static string CreateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        const string ALPHANUMERICCHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();

        return new string(Enumerable.Repeat(ALPHANUMERICCHARS, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
