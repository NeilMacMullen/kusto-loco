using System.Linq;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class CaseFunctionImpl<T> : IScalarFunctionImpl

{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var val = arguments.Last().Value is T?
            ? (T?)arguments.Last().Value
            : default;
        for (var p = 0; p < arguments.Length / 2; p++)
        {
            var pred = arguments[p * 2].Value as bool?;
            if (pred == true)
            {
                val = (T?)arguments[p * 2 + 1].Value;
                break;
            }
        }

        var typeSymbol = TypeMapping.SymbolForType(typeof(T));
        return new ScalarResult(typeSymbol, val);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var caseLength = arguments.Length / 2;
        var fallback = (TypedBaseColumn<T?>)arguments.Last().Column;
        var condValues = Enumerable.Range(0, caseLength)
            .Select(i => new
            {
                Pred = (TypedBaseColumn<bool?>)arguments[i * 2].Column,
                Val = (TypedBaseColumn<T?>)arguments[i * 2 + 1].Column
            })
            .ToArray();


        var rowCount = fallback.RowCount;
        var data = NullableSetBuilderLocator.GetFixedNullableSetBuilderForType(typeof(T), rowCount);
        for (var i = 0; i < rowCount; i++)
        {
            var val = fallback[i];
            foreach (var c in condValues)
                if (c.Pred[i]!.Value)
                {
                    val = c.Val[i];
                    break;
                }

            data.Add(val);
        }

        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
    }
}
