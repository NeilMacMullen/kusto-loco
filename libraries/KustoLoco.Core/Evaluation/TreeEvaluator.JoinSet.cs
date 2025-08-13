using System.Collections.Generic;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public class JoinSet
    {
        public readonly List<int> RowNumbers = [];

        public int RowCount => RowNumbers.Count;

        public void Add(int row)
        {
            RowNumbers.Add(row);
        }
    }
}
