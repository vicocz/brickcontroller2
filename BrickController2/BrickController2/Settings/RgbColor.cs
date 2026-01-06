using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace BrickController2.Settings;

/// <summary>
/// Represents serializable RGB color for settings.
/// </summary>
[TypeConverter(typeof(RgbColorConverter))]
[JsonConverter(typeof(RgbColorJsonConverter))]
public readonly struct RgbColor
{
    /// <summary>
    /// The red component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    public readonly float R;

    /// <summary>
    /// The green component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    public readonly float G;

    /// <summary>
    /// The blue component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    public readonly float B;

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

    /// <summary>
    /// Adjusts the Value (Brightness) and returns a new RgbColor.
    /// </summary>
    public RgbColor WithValueFactor(float factor)
    {
        // 1. Convert to HSV (All values 0.0 - 1.0)
        ToHsv(out var h, out var s, out var v);

        // 2. Adjust V (Clamp ensures we don't exceed 1.0 or drop below 0.0)
        float newValue = Math.Clamp(v * factor, 0f, 1f);

        // 3. Convert back to RGB
        return FromHsv(h, s, newValue);
    }

    public void ToHsv(out float hue, out float saturation, out float value)
    {
        float max = Math.Max(R, Math.Max(G, B));
        float min = Math.Min(R, Math.Min(G, B));
        float delta = max - min;

        hue = 0f;
        value = max;
        saturation = (max == 0) ? 0 : (delta / max);

        if (delta != 0)
        {
            hue = max switch
            {
                _ when max == R => (G - B) / delta,
                _ when max == G => 2f + (B - R) / delta,
                _ => 4f + (R - G) / delta,
            };

            // Normalize Hue to 0.0 - 1.0 range (instead of multiplying by 60)
            hue /= 6f;

            if (hue < 0f) hue += 1f;
        }
    }

    public static RgbColor FromHsv(float h, float s, float v)
    {
        // Hue is 0.0 - 1.0, but the math is easier in 0-6 sectors
        float hSector = h * 6f;

        float c = v * s;
        float x = c * (1 - Math.Abs(hSector % 2 - 1));
        float m = v - c;

        // Map the 0-6 sector to RGB distribution
        return hSector switch
        {
            >= 0 and < 1 => new RgbColor(c + m, x + m, m),
            >= 1 and < 2 => new RgbColor(x + m, c + m, m),
            >= 2 and < 3 => new RgbColor(m, c + m, x + m),
            >= 3 and < 4 => new RgbColor(m, x + m, c + m),
            >= 4 and < 5 => new RgbColor(x + m, m, c + m),
            _ => new RgbColor(c + m, m, x + m), // Sector 5-6
        };
    }

    public int ToInt() => (int)(R * 255) << 16 |
        (int)(G * 255) << 8 |
        (int)(B * 255);

}
