using BrickController2.UI.Services.Translation;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BrickController2.UI.Converters;

/// <summary>
/// Converts an authentication token to a placeholder text if the token is null or empty.
/// </summary>
public class NoAuthTokenToPlaceholderConverter : IValueConverter
{
    private readonly ITranslationService _translationService;
    
    public NoAuthTokenToPlaceholderConverter(ITranslationService translationService)
    {
        _translationService = translationService;
    }   

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        return string.IsNullOrWhiteSpace(text) ? _translationService.Translate("NoAuthToken") : text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value;
}
