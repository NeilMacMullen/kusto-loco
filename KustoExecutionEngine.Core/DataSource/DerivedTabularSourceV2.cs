using System;
using System.Collections.Generic;

namespace KustoExecutionEngine.Core
{
    public class DerivedTabularSourceV2 : ITabularSourceV2
    {
        private readonly ITabularSourceV2 _source;
        private readonly TableSchema _newSchema;
        private readonly Func<ITableChunk, ITableChunk?> _mapFn;

        public DerivedTabularSourceV2(ITabularSourceV2 source, TableSchema newSchema, Func<ITableChunk, ITableChunk?> mapFn)
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
                if (newChunk != null)
                {
                    yield return newChunk;
                }
            }
        }
    }
}
