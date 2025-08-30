using System.Globalization;
using Avalonia.Data.Converters;
using NotNullStrings;

namespace LokqlDx.Converters;

internal class BoolChooseStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && parameter is string choiceString)
        {
            var choices = choiceString.Tokenize("|");
            if (choices.Length == 2)
                return b ? choices[0] : choices[1];
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
