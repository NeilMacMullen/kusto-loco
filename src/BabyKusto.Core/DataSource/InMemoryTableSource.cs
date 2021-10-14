// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace BabyKusto.Core
{
    public class InMemoryTableSource : ITableSource
    {
        private readonly ITableChunk[] _data;

        public InMemoryTableSource(TableSchema schema, Column[] columns)
        {
            this.Schema = schema;
            this._data = new ITableChunk[] { new TableChunk(Schema, columns) };
        }

        public TableSchema Schema { get; private set; }

        public IEnumerable<ITableChunk> GetData() => _data;
    }
}
