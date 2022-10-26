using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BabyKusto.Core;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace BabyKusto.ProcessQuerier
{
    public class ProcessesTable : ITableSource
    {
        public ProcessesTable(string tableName)
        {
            Type = new TableSymbol(
                tableName,
                new ColumnSymbol("pid", ScalarTypes.Int),
                new ColumnSymbol("name", ScalarTypes.String),
                new ColumnSymbol("numThreads", ScalarTypes.Int),
                new ColumnSymbol("workingSet", ScalarTypes.Long)
            );
        }

        public TableSymbol Type { get; }

        public IEnumerable<ITableChunk> GetData()
        {
            var pids = new ColumnBuilder<int?>(ScalarTypes.Int);
            var names = new ColumnBuilder<string?>(ScalarTypes.String);
            var numThreads = new ColumnBuilder<int?>(ScalarTypes.Int);
            var workingSets = new ColumnBuilder<long?>(ScalarTypes.Long);

            foreach (var p in Process.GetProcesses())
            {
                pids.Add(p.Id);
                names.Add(p.ProcessName);
                numThreads.Add(p.Threads.Count);
                workingSets.Add(p.WorkingSet64);
            }

            var builders = new ColumnBuilder[] { pids, names, numThreads, workingSets };
            yield return new TableChunk(this, builders.Select(b => b.ToColumn()).ToArray());
        }

        public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
        {
            throw new NotSupportedException();
        }
    }
}