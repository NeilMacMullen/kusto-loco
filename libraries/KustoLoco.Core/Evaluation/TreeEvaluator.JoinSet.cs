using System.Collections.Generic;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public class JoinSet
    {
        private ITableChunk _chunk = null!;
        private readonly List<int> _rowNumbers = [];

        public int RowCount => _rowNumbers.Count;

        public void Add(ITableChunk chunk, int row)
        {
            if (_rowNumbers.Count == 0)
                _chunk = chunk;
            _rowNumbers.Add(row);
        }

        public IEnumerable<(ITableChunk, int)>
            Enumerate()
        {
            foreach (var row in _rowNumbers)
                yield return (_chunk, row);
        }

        public (ITableChunk, int) GetFirst() => (_chunk, _rowNumbers[0]);
    }
}
