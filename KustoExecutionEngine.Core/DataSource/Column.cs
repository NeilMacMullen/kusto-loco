namespace KustoExecutionEngine.Core
{
    public class Column
    {
        private readonly object?[] _data;

        public Column(object?[] data)
        {
            _data = data;
        }

        public int RowCount => _data.Length;

        public object? this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
    }
}
