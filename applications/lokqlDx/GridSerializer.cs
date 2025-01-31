using System.Windows;
using System.Windows.Controls;
using NotNullStrings;

namespace lokqlDx;

public class GridSerializer
{
    public static string[] Serialize(Grid grid)
    {
        var converter = new GridLengthConverter();
        var rows = grid.RowDefinitions
                .Select(row => converter.ConvertToInvariantString(row.Height).NullToEmpty())
            ;
        var cols = grid.ColumnDefinitions
                .Select(col => converter.ConvertToInvariantString(col.Width).NullToEmpty())
            ;
        return rows.Concat(cols).ToArray();
    }

    public static void DeSerialize(Grid grid, string[] items)
    {
        try
        {
            var converter = new GridLengthConverter();
            var index = 0;
            foreach (var row in grid.RowDefinitions)
            {
                if (index < items.Length)
                    if (converter.ConvertFromInvariantString(items[index]) is GridLength len)
                        row.Height = len;
                index++;
            }
            foreach (var col in grid.ColumnDefinitions)
            {
                if (index < items.Length)
                    if (converter.ConvertFromInvariantString(items[index]) is GridLength len)
                        col.Width = len;
                index++;
            }
        }
        catch
        {
        }
    }
}
