using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class BetweenOperatorImpl<T> : IScalarFunctionImpl
    where T : struct, IComparable<T>

{
    private readonly bool _invert;

    public BetweenOperatorImpl(bool invert) => _invert = invert;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        var r = (T?)arguments[0].Value;
        var left = (T?)arguments[1].Value;
        var right = (T?)arguments[2].Value;
        return new ScalarResult(ScalarTypes.Bool, Impl(r, left, right));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);

        var row = (TypedBaseColumn<T?>)arguments[0].Column;
        var left = (TypedBaseColumn<T?>)arguments[1].Column;
        var right = (TypedBaseColumn<T?>)arguments[2].Column;
        var data = NullableSetBuilderOfbool.CreateFixed(row.RowCount);

        var rangePartitioner = Partitioner.Create(0, left.RowCount, 1000);

        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var r = row[i];
                var lefts = left[i];
                var rights = right[i];
                data[i] = Impl(r, lefts, rights);
            }
        });


        return new ColumnarResult(GenericColumnFactoryOfbool.CreateFromDataSet(data.ToNullableSet()));
    }

    private bool? Impl(T? r, T? left, T? right) =>
        r.HasValue && left.HasValue && right.HasValue
        && (r.Value.CompareTo(left.Value) >= 0
            && r.Value.CompareTo(right.Value) <= 0)
        ^ _invert;
}

internal class IntBetweenOperatorImpl : IScalarFunctionImpl

{
    private readonly bool _invert;

    public IntBetweenOperatorImpl(bool invert) => _invert = invert;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        var r = (int?)arguments[0].Value;
        var left = (long?)arguments[1].Value;
        var right = (long?)arguments[2].Value;
        return new ScalarResult(ScalarTypes.Bool, Impl(r, left, right));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);

        var row = (TypedBaseColumn<int?>)arguments[0].Column;
        var left = (TypedBaseColumn<long?>)arguments[1].Column;
        var right = (TypedBaseColumn<long?>)arguments[2].Column;
        var data = NullableSetBuilderOfbool.CreateFixed(row.RowCount);

        var rangePartitioner = Partitioner.Create(0, left.RowCount, 1000);

        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var r = row[i];
                var lefts = left[i];
                var rights = right[i];
                data[i] = Impl(r, lefts, rights);
            }
        });


        return new ColumnarResult(GenericColumnFactoryOfbool.CreateFromDataSet(data.ToNullableSet()));
    }


    private bool? Impl(int? r, long? left, long? right) =>
        r.HasValue && left.HasValue && right.HasValue
        && (r >= left
            && r <= right)
        ^ _invert;
}

internal class BetweenOperatorDateTimeWithTimespanImpl : IScalarFunctionImpl
{
    private readonly bool _invert;

    public BetweenOperatorDateTimeWithTimespanImpl(bool invert) => _invert = invert;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);
        var r = (DateTime?)arguments[0].Value;
        var left = (DateTime?)arguments[1].Value;
        var right = (TimeSpan?)arguments[2].Value;
        return new ScalarResult(ScalarTypes.Bool, Impl(r, left, right));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 3);

        var row = (TypedBaseColumn<DateTime?>)arguments[0].Column;
        var left = (TypedBaseColumn<DateTime?>)arguments[1].Column;
        var right = (TypedBaseColumn<TimeSpan?>)arguments[2].Column;
        var data = NullableSetBuilderOfbool.CreateFixed(row.RowCount);

        var rangePartitioner = Partitioner.Create(0, left.RowCount, 1000);

        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var r = row[i];
                var lefts = left[i];
                var rights = right[i];
                data[i] = Impl(r, lefts, rights);
            }
        });


        return new ColumnarResult(GenericColumnFactoryOfbool.CreateFromDataSet(data.ToNullableSet()));
    }

    private bool? Impl(DateTime? r, DateTime? left, TimeSpan? right) =>
        r.HasValue && left.HasValue && right.HasValue
        && (r >= left
            && r <= left + right)
        ^ _invert;
}
