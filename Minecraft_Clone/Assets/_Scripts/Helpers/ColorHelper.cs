using UnityEngine;

public static class ColorHelper
{
    public static string ToRGBHex(this Color color)
        => $"{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}";

    private static byte ToByte(float value) => (byte)(value * 255);

    /// <summary>
    /// Wrap a string to rich text string with color
    /// </summary>
    public static string RichText(this string s, Color color)
    {
        return $"<color=#{ToRGBHex(color)}>{s}</color>";
    }
}

