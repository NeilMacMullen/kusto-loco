using System;
using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class RandFunctionImpl : IScalarFunctionImpl
{
    private readonly Random _random = new();

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var scale = arguments.Any()
            ? arguments[0].Value as double?
            : 1.0;
        var r = GetRand(scale);
        return new ScalarResult(ScalarTypes.Real, r);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);
        //the first column is a "dummy" column inserted by the evaluator
        //because we specified this as "ForceColumnarResult"
        var column = (TypedBaseColumn<int?>)arguments[0].Column;
       
        if (arguments.Length > 1)
        {
            var nColumn = (TypedBaseColumn<double?>)arguments[1].Column;
            var length = nColumn.RowCount;
            var data = NullableSetBuilderOfdouble.CreateFixed(length);
       
            for (var i = 0; i < length; i++)
            {
                var max = nColumn[i];
                data[i] = GetRand(max);
            }
            return new ColumnarResult(GenericColumnFactoryOfdouble.CreateFromDataSet(data.ToNullableSet()));
        }
        else
        {
            var data = NullableSetBuilderOfdouble.CreateFixed(column.RowCount);
            for (var i = 0; i < column.RowCount; i++) data[i] = GetRand(0);
            return new ColumnarResult(GenericColumnFactoryOfdouble.CreateFromDataSet(data.ToNullableSet()));
        }
    }

    private double? GetRand(double? scale) =>
        //the behaviour of adx seems to be that invalid values
        //give Real results 0..1
        scale switch
        {
            null => null,
            <= 1.0 => _random.NextDouble(),
            _ => _random.Next((int)scale)
        };
}
