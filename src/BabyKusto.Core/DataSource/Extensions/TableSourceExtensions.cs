using System.IO;

namespace BabyKusto.Core.Extensions
{
    public static class TableSourceExtensions
    {
        public static void Dump(this ITableSource table, TextWriter writer, int indent = 0)
        {
            WriteIndent(writer, indent);
            foreach (var columnDef in table.Schema.ColumnDefinitions)
            {
                writer.Write(columnDef.ColumnName);
                writer.Write("; ");
            }
            writer.WriteLine();

            WriteIndent(writer, indent);
            writer.WriteLine("------------------");

            foreach (var chunk in table.GetData())
            {
                for (int i = 0; i < chunk.RowCount; i++)
                {
                    WriteIndent(writer, indent);
                    var row = chunk.GetRow(i);
                    for (int j = 0; j < row.Values.Length; j++)
                    {
                        writer.Write(row.Values[j]);
                        writer.Write("; ");
                    }
                    writer.WriteLine();
                }
            }
        }

        public static string DumpToString(this ITableSource table, int indent = 0)
        {
            using var writer = new StringWriter();
            table.Dump(writer, indent);
            return writer.ToString();
        }


        private static void WriteIndent(TextWriter writer, int indent)
        {
            if (indent > 0)
            {
                writer.Write(new string(' ', indent));
            }
        }
    }
}