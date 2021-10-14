using BabyKusto.Core;

namespace BabyKusto.BlazorApp.Data
{
    public class FileProcessor
    {
        public object? fileProcessor(string fileName, string uploadedFilePath, string query)
        {
            var columnName = Array.Empty<string>();
            var rowValues = Array.Empty<object?>();
            var columns = new List<List<object?>>();

            try
            {
                using (StreamReader sr = new StreamReader(uploadedFilePath))
                {
                    string line;

                    // Read line by line
                    var rowNum = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (rowNum == 0)
                        {
                            // ['a', 'b']
                            columnName = line.Split(',');

                            // Initialize column list
                            var c = 0;
                            while (c < columnName.Length)
                            {
                                columns.Add(new List<object?>());
                                c++;
                            }
                        }
                        else
                        {
                            // [1.0, 2.0]
                            rowValues = line.Split(',');

                            // Fill column array
                            var r = 0;
                            while (r < rowValues.Length)
                            {
                                columns[r].Add(rowValues[r]);
                                r++;
                            }
                        }
                        rowNum++;
                    }
                }

                // Generate ColumnDefinition
                var tableSchema = new List<ColumnDefinition>();

                foreach (var colmunValue in columnName)
                {
                    tableSchema.Add(new ColumnDefinition(colmunValue, KustoValueKind.Real));
                }


                var myTable = new InMemoryTableSource(
                    new TableSchema(
                    tableSchema),
                    columns.Select(c => new Column(c.ToArray())).ToArray());

                // Call engine
                var engine = new BabyKustoEngine();
                engine.AddGlobalTable(
                fileName,
                myTable);
                return engine.Evaluate(query);
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return "Please refer console for more details.";
            }
        }
    }
}
