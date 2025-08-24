using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LokqlDx.Converters;

public class BoolToBrushMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3)
            return null;

        var boolValue = values[0] as bool? ?? false;
        var falseBrush = values[1] as IBrush;
        var trueBrush = values[2] as IBrush;

        return boolValue ? trueBrush : falseBrush;
    }
}
