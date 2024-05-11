using System;
using System.Drawing;
using System.Globalization;
using System.Numerics;

namespace Automaton.Utils;
public static class Extensions
{
    public static uint Reverse(this uint value)
        => ((value & 0x000000FFu) << 24) | ((value & 0x0000FF00u) << 8) |
            ((value & 0x00FF0000u) >> 8) | ((value & 0xFF000000u) >> 24);

    public static int RoundOff(this int i, int sliderIncrement) => (int)Math.Round(i / Convert.ToDouble(sliderIncrement)) * sliderIncrement;
    public static float RoundOff(this float i, float sliderIncrement) => (float)Math.Round(i / sliderIncrement) * sliderIncrement;

    public static Vector2 Vec2(this int i) => new(i);
    public static Vector2 ToVec2(this Point p) => new(p.X, p.Y);
    public static Point ToPoint(this Vector2 v) => new((int)Math.Round(v.X), (int)Math.Round(v.Y));

    public static int Ms(this int i) => i * 1000;

    public static string ToTitleCase(this string s) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
    public static string GetLast(this string source, int tail_length) => tail_length >= source.Length ? source : source[^tail_length..];
}
