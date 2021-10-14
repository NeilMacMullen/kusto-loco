using System;
using KustoExecutionEngine.Core;

namespace KustoExecutionEngine.Cli
{
    internal static class TableSourceExtensions
    {
        internal static void DumpToConsole(this ITableSource table, int indent = 0)
        {
            WriteIndent(indent);
            foreach (var columnDef in table.Schema.ColumnDefinitions)
            {
                Console.Write(columnDef.ColumnName);
                Console.Write("; ");
            }
            Console.WriteLine();

            WriteIndent(indent);
            Console.WriteLine("------------------");

            foreach (var chunk in table.GetData())
            {
                for (int i = 0; i < chunk.RowCount; i++)
                {
                    WriteIndent(indent);
                    var row = chunk.GetRow(i);
                    for (int j = 0; j < row.Values.Length; j++)
                    {
                        Console.Write(row.Values[j]);
                        Console.Write("; ");
                    }
                    Console.WriteLine();
                }
            }
        }

        private static void WriteIndent(int indent)
        {
            if (indent > 0)
            {
                Console.Write(new string(' ', indent));
            }
        }
    }
}