using Newtonsoft.Json;
using System.ComponentModel;

namespace BrickController2.Settings;

/// <summary>
/// Represents serializable RGB color for settings.
/// </summary>
[TypeConverter(typeof(RgbColorConverter))]
[JsonConverter(typeof(RgbColorJsonConverter))]
public struct RgbColor
{
    /// <summary>
    /// The red component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    public float R;

    /// <summary>
    /// The green component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    public float G;

    /// <summary>
    /// The blue component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    public float B;

    public RgbColor()
    {
    }

    public RgbColor(int colorValue)
    {
        R = ((colorValue >> 16) & 0xFF) / 255f;
        G = ((colorValue >> 8) & 0xFF) / 255f;
        B = (colorValue & 0xFF) / 255f;
    }

    public RgbColor(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }

    internal readonly int ColorValue => (int)(R * 255) << 16 |
        (int)(G * 255) << 8 |
        (int)(B * 255);

}
