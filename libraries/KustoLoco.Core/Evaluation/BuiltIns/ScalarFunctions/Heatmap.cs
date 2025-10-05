using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class Heatmap
{
    public static string GetHeatmapColor(string type, double min, double max, double value)
    {
        // Normalize to [0,1]
        double t = (value - min) / (max - min);
        t = Math.Clamp(t, 0.0, 1.0);

        return type.ToLowerInvariant() switch
        {
            "grayscale" => Grayscale(t),
            "gradient" => HeatmapGradient(t),
            "hue" => HueRotation(t),
            _ => HeatmapGradient(t)
        };
    }

    private static string Grayscale(double t)
    {
        int intensity = (int)(t * 255);
        return ToHex(intensity, intensity, intensity);
    }

    private static string HeatmapGradient(double t)
    {
        // Define palette stops
        var palette = new (int r, int g, int b)[]
        {
            (0,   0,   128),   // dark blue
            (0,   0,   255),   // blue
            (0,   255, 0),     // green
            (255, 255, 0),     // yellow
            (255, 0,   0),     // red
            (255, 255, 255)    // white
        };

        double scaled = t * (palette.Length - 1);
        int idx = (int)Math.Floor(scaled);
        double frac = scaled - idx;

        if (idx >= palette.Length - 1)
            return ToHex(palette[^1].r, palette[^1].g, palette[^1].b);

        var c1 = palette[idx];
        var c2 = palette[idx + 1];

        int r = (int)(c1.r + (c2.r - c1.r) * frac);
        int g = (int)(c1.g + (c2.g - c1.g) * frac);
        int b = (int)(c1.b + (c2.b - c1.b) * frac);

        return ToHex(r, g, b);
    }

    private static string HueRotation(double t)
    {
        // Hue from 240° (blue) down to 0° (red)
        double hue = 240.0 * (1.0 - t);
        return HsvToHex(hue, 1.0, 1.0);
    }

    private static string HsvToHex(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = v - c;

        double r1, g1, b1;
        if (h < 60) { r1 = c; g1 = x; b1 = 0; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
        else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }

        int r = (int)((r1 + m) * 255);
        int g = (int)((g1 + m) * 255);
        int b = (int)((b1 + m) * 255);

        return ToHex(r, g, b);
    }

    private static string ToHex(int r, int g, int b) =>
        $"#{r:X2}{g:X2}{b:X2}";
}
