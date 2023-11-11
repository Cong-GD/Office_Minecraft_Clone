using UnityEngine;

public static class ColorHelper
{
    public static string ToRGBHex(this Color color)
        => string.Format("{0:X2}{1:X2}{2:X2}", ToByte(color.r), ToByte(color.g), ToByte(color.b));

    private static byte ToByte(float value) => (byte)(value * 255);

    /// <summary>
    /// Wrap a string to rich text string with color
    /// </summary>
    public static string RichText(this string s, Color color)
    {
        return $"<color=#{ToRGBHex(color)}>{s}</color>";
    }
}