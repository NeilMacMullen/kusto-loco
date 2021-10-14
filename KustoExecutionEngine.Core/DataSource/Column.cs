namespace KustoExecutionEngine.Core.DataSource
{
    public class Column
    {
        private readonly object?[] _data;

        public Column(int size)
        {
            _data = new object?[size];
        }

        public int RowCount => _data.Length;

        public object? this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
    }
}
