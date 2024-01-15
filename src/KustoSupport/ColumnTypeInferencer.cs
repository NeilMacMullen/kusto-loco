using System.Globalization;
using BabyKusto.Core;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using NLog;

namespace KustoSupport;

public static class ColumnTypeInferencer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static BaseColumn AutoInfer(BaseColumn source)
    {
        if (source.Type != ScalarTypes.String)
            return source;

        var stringColumn = (TypedBaseColumn<string>)source;
        var typeTriers = new TypeTryer[]
        {
            //don't bother with int .... Kusto is natively "long"
            //so this would just lead to excessive casts
            new(typeof(long), s => (long.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
            new(typeof(double), s => (double.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
            new(typeof(DateTime), s => (DateTime.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
            new(typeof(Guid), s => (Guid.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
            new(typeof(TimeSpan), s => (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
            new(typeof(bool), s => (bool.TryParse(s, out var i), i)),
        };
        foreach (var t in typeTriers)
        {
            var processedAll = true;
            var builder = ColumnHelpers.CreateBuilder(t.Type);
            for (var i = 0; i < stringColumn.RowCount; i++)
            {
                var cell = stringColumn[i];
                //blank cells tell us nothing about type since data may
                //be missing
                if (string.IsNullOrWhiteSpace(cell))
                {
                    builder.Add(null);
                    continue;
                }

                var (parsed, val) = t.parser(cell);
                if (parsed)
                    builder.Add(val);
                else
                {
                    processedAll = false;
                    break;
                }
            }

            if (processedAll)
                return builder.ToColumn();
        }

        //if we ran out of conversions to try, just return the original column 
        return source;
    }

    private readonly record struct TypeTryer(Type Type, Func<string, (bool, object)> parser);
}