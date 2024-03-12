using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class RandFunctionImpl : IScalarFunctionImpl

{
    private readonly Random _random = new();

    public ScalarResult InvokeScalar(ScalarResult[] arguments) => new(ScalarTypes.Real, _random.NextDouble());

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);
        //the first column is a "dummy" column inserted by the evaluator
        //because we specified this as "ForceColumnarResult"
        var column = (TypedBaseColumn<int?>)arguments[0].Column;
        var data = new double?[column.RowCount];
        if (arguments.Length > 1)
        {
            var nColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            for (var i = 0; i < column.RowCount; i++)
            {
                //the behaviour of adx seems to be that invalid values
                //give Real results 0..1
                var max = nColumn[i];
                data[i] = max is > 1
                    ? _random.Next((int)max.Value)
                    : _random.NextDouble();
            }
        }
        else
        {
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = _random.NextDouble();
            }
        }


        return new ColumnarResult(ColumnFactory.Create(data));
    }
}