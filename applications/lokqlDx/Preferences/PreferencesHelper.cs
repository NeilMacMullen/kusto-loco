using System.Globalization;
using Avalonia.Media;

namespace LokqlDx;

public static class PreferencesHelper
{
    public static FontFamily? GetFirstMonospaceFontFamily()
    {
        var fonts = FontManager.Current.SystemFonts
            .OrderBy(x => x.Name)
            .ToList();
        
        return fonts.FirstOrDefault(IsMonospaced);
    }

    private static readonly char[] _testChars = ['W', 'I', '1', '*'];
    public static bool IsMonospaced(FontFamily fontFamily)
    {
        var typeface = new Typeface(fontFamily, FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);
        var n = 50;

        var texts = _testChars
            .Select(x => new FormattedText(
                string.Empty.PadRight(n, x),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                20,
                Brushes.Black))
            .Select(x => x.WidthIncludingTrailingWhitespace)
            .ToList();
        
        var first = texts.First();
        return texts.All(x => x == first);
    }
}
