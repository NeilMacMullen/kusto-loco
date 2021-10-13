using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KustoExecutionEngine.Core.DataSource
{
    public class Column
    {
        private readonly object[] _data;

        public Column(int size)
        {
            this._data = new object[size];
        }

        public int Size => this._data.Length;

        public object this[int index]
        {
            get => this._data[index];
            set => this._data[index] = value;
        }
    }
}
