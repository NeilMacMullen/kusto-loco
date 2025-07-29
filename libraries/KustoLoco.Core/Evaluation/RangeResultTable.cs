using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation;

[KustoGeneric(Types = "numeric")]
public class RangeResultTable<T> : ITableSource
    where T : struct, INumber<T>
{
    private const int MaxRowsPerChunk = 100_000;
    private readonly T _from;
    private readonly T _step;
    private readonly T _to;

    public RangeResultTable(ScalarResult from, ScalarResult to, ScalarResult step, TableSymbol resultType)
    {
        Type = resultType;
        if (from.Value is null || to.Value is null || step.Value is null)
        {
            // Return empty table
            _from = T.One;
            _to = T.Zero;
            _step = T.One;
            return;
        }

        _from = GetValue(from.Value);
        _to = GetValue(to.Value);
        _step = GetValue(step.Value);
    }
    private static T GetValue(object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }
    public TableSymbol Type { get; }

    public IEnumerable<ITableChunk> GetData()
    {
        var direction = _to >= _from ? T.One : -T.One;
        var stepDirection = _step >= T.Zero ? T.One : -T.One;

        if (_step == T.Zero || direction != stepDirection)
        {
            yield return new TableChunk(this,
                [GenericColumnFactory<T>.CreateFromObjects([])]);
            yield break;
        }

        Func< T , bool> isDone = direction == T.One //GENERIC INLINE
            ? val => val <= _to
            : val => val >= _to;

        
        var i = 0;
        var chunk = NullableSetBuilder<T>.CreateExpandable(0);
        for (var val = _from; isDone(val); val += _step)
        {
            
            chunk.Add(val);
            i++;
            if (i != MaxRowsPerChunk)
                continue;

            yield return new TableChunk(this,
                [GenericColumnFactory<T>.CreateFromDataSet(chunk.ToNullableSet())]);
            i = 0;
            chunk = NullableSetBuilder<T>.CreateExpandable(0);
        }

        if (i <= 0)
            yield break;

        yield return new TableChunk(this, [GenericColumnFactory<T>.CreateFromDataSet(chunk.ToNullableSet())]);
    }

    public IAsyncEnumerable<ITableChunk> GetDataAsync(
        CancellationToken cancellation = default)
    {
        return GetData().ToAsyncEnumerable();
    }
}
