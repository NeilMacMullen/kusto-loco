using System;
using System.Globalization;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core;


/// <summary>
/// Tries to infer the type of the column based on its contents
/// </summary>
/// <remarks>
/// Inferring the type is error-prone. Excel is infamous for example, for turning things that look like
/// long sequences of digits into floating point numbers and throwing away the significant bits!
/// </remarks>
public static class ColumnTypeInferrer
{
    private static readonly TypeTrier [] TypeTriers =
    [
        //don't bother with int .... Kusto is natively "long"
        //so this would just lead to excessive casts
        new(typeof(long), s => (long.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
        new(typeof(double), s => (s.Length <=17 || s.Contains('.')) && double.TryParse(s, CultureInfo.InvariantCulture, out var i) ? (true,i) :(false,0)),
        new(typeof(DateTime), s => (DateTime.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
        new(typeof(Guid), s => (Guid.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
        new(typeof(TimeSpan), s => (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out var i), i)),
        new(typeof(bool), s => (bool.TryParse(s, out var i), i))
    ];
    
    public static BaseColumn AutoInfer(BaseColumn source)
    {
        if (source.Type != ScalarTypes.String)
            return source;

        var stringColumn = (GenericTypedBaseColumnOfstring)source;

     
        //attempt each type in turn until we find one that works
        foreach (var t in TypeTriers)
        {
            var processedAll = true;
            var builder = ColumnHelpers.CreateBuilder(t.Type,source.Name);
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

                var (parsed, val) = t.Parser(cell);
                if (parsed)
                    builder.Add(val);
                else
                {
                    //one failure is enough to give up on this type
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

    private readonly record struct TypeTrier(Type Type, Func<string, (bool, object)> Parser);
}
