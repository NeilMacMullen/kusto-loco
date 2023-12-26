using System;
using System.Linq;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

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

        return new ScalarResult(ScalarTypeFromNetType(typeof(T)), val);
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
        var data = new T?[rowCount];
        for (var i = 0; i < rowCount; i++)
        {
            var val = fallback[i];
            foreach (var c in condValues)
            {
                if (c.Pred[i]!.Value)
                {
                    val = c.Val[i];
                    break;
                }
            }

            data[i] = val;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static TypeSymbol ScalarTypeFromNetType(Type t)
    {
        if (t == typeof(string))
            return ScalarTypes.String;
        if (t == typeof(int?))
            return ScalarTypes.Int;
        if (t == typeof(long?))
            return ScalarTypes.Long;
        if (t == typeof(double?))
            return ScalarTypes.Real;
        if (t == typeof(DateTime?))
            return ScalarTypes.DateTime;
        if (t == typeof(bool?))
            return ScalarTypes.Bool;


        throw new NotImplementedException($"Don't know what to do with type {t.Name}");
    }
}