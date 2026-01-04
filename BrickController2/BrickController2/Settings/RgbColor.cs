using Newtonsoft.Json;

namespace BrickController2.Settings;

/// <summary>
/// Represents serializable RGB color for settings.
/// </summary>
public struct RgbColor
{
    [JsonProperty]
    /// </summary>
    public float R;

    /// <summary>
    /// The green component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    [JsonProperty]
    public float G;

    /// <summary>
    /// The blue component of the color, ranging from 0.0 to 1.0.
    /// </summary>
    [JsonProperty]
    public float B;
}
