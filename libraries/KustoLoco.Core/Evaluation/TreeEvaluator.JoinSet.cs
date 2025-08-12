using System;
using System.Collections.Generic;
using System.Linq;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public class JoinSet()
    {
        private ITableChunk[] _chunks= [];
        private List<int>[] _rowNums =[];

        public void Add(ITableChunk chunk, int row)
        {
            if (!_chunks.Contains(chunk))
            {
                _chunks = _chunks.Prepend(chunk).ToArray();
                _rowNums = _rowNums.Prepend([]).ToArray();
            }
            _rowNums[0].Add(row);
        }

        public int RowCount => _rowNums.Sum(i => i.Count);

        public (ITableChunk chunk, int row) Get(int index)
        {
            var chunkIndex = 0;
            foreach (var list in _rowNums)
            {
                if (index < list.Count)
                {
                    return (_chunks[chunkIndex], list.ElementAt(index));
                }

                index -= list.Count;
                chunkIndex++;
            }

            throw new InvalidOperationException();
        }

        public IEnumerable<(ITableChunk, int)>
            Enumerate()
        {
            var cnt = _rowNums.Length;
            for (var i = 0; i < cnt; i++)
            {
                foreach (var row in _rowNums[i])
                {
                    yield return (_chunks[i], row);
                }
            }
        }
    }
}
