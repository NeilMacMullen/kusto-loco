using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.DataSource;

/// <summary>
///     The NullTableSource used as a placeholder
/// </summary>
/// <remarks>
///     The null table source is NOT the same thing as an EMPTY table
///     since an empty table might still contain column information
/// </remarks>
public class NullTableSource : ITableSource
{
    public static readonly NullTableSource Instance = new();
    public TableSymbol Type { get; } = TableSymbol.Empty;
    public IEnumerable<ITableChunk> GetData() => [];

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
        => AsyncEnumerable.Empty<ITableChunk>();
}
