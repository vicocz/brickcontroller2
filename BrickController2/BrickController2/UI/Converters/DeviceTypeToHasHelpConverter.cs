using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Help;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BrickController2.UI.Converters;

/// <summary>
/// Converts a DeviceType to true when a help document exists for the device.
/// </summary>
public class DeviceTypeToHasHelpConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DeviceType deviceType && HelpResources.Exists(HelpTopic.ForDevice(deviceType));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
