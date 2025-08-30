using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LokqlDx.Converters;

public class BoolToBrushMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var bools = values.OfType<bool>().ToArray();
        var brushes = values.OfType<IBrush>().ToArray();
        if (!bools.Any()) return brushes.FirstOrDefault();

        if (!brushes.Any())
            return null;
        var index = 0;
        foreach (var b in bools)
        {
            index = index << 1;
            if (b) index |= 1;
        }

        return brushes[Math.Min(index, brushes.Length)];
    }
}
