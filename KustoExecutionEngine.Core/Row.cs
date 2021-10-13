using System.Collections;
using System.Collections.Generic;

namespace KustoExecutionEngine.Core
{
    public class Row : IRow
    {
        private readonly Dictionary<string, object?> _values;

        public Row(IEnumerable<KeyValuePair<string, object?>> values)
        {
            _values = new Dictionary<string, object?>(values);
        }

        public object? this[string columnName] => _values[columnName];

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            foreach (var kvp in _values)
            {
                yield return kvp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
