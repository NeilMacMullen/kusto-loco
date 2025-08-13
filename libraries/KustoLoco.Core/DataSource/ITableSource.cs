//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core;

public interface ITableSource
{
    TableSymbol Type { get; }
    public IEnumerable<string> ColumnNames => Type.Columns.Select(c => c.Name);
    public string Name => Type.Name;

    IEnumerable<ITableChunk> GetData();
}

public interface IMaterializedTableSource : ITableSource
{
    public int RowCount { get; }
    public ITableChunk[] Chunks { get; }
    public int ChunkCount => Chunks.Length;
}
