using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KustoExecutionEngine.Core
{
    public class DerivedTableSource : ITableSource
    {
        private readonly ITableSource _source;
        private readonly TableSchema _newSchema;
        private readonly Func<ITableChunk, ITableChunk?> _mapFn;

        public DerivedTableSource(ITableSource source, TableSchema newSchema, Func<ITableChunk, ITableChunk?> mapFn)
        {
            _source = source;
            _newSchema = newSchema;
            _mapFn = mapFn;
        }

        public TableSchema Schema => this._newSchema;

        public IEnumerable<ITableChunk> GetData()
        {
            foreach (var chunk in this._source.GetData())
            {
                var newChunk = _mapFn(chunk);
                if (newChunk is not null)
                {
                    Debug.Assert(ReferenceEquals(newChunk.Schema, _newSchema));
                    yield return newChunk;
                }
            }
        }
    }
}
