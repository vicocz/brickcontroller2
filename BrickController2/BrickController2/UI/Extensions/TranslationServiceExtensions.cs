using BrickController2.UI.Services.Translation;
using System;

namespace BrickController2.UI.Extensions;

internal static class TranslationServiceExtensions
{
    public static string Translate(this ITranslationService service,  string key, string extra)
        => service.Translate(key) + " " + extra;
    public static string Translate(this ITranslationService service, string key, Exception ex)
        => service.Translate(key, ex.Message);
    public static string Translate<T>(this ITranslationService service, T key) where T : Enum
        => service.Translate(key.ToString());
}
