using System.Data;
using System.Text.Json.Nodes;
using KustoLoco.Core;
using KustoLoco.Core.Util;

namespace AppInsightsSupport;

public static class TableLoaderFromIDataReader
{
    public static IMaterializedTableSource LoadTable(string tableName, IDataReader reader)
    {
        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(i => new ColumnResult(reader.GetName(i), i,BodgeGetFieldType(i)))
            .ToArray();
        var columnBuilders
            = columns.Select(c => ColumnHelpers.CreateBuilder(c.UnderlyingType, c.Name))
                .ToArray();

        while (reader.Read())
        {
            var row = new object[reader.FieldCount];
            reader.GetValues(row);
            foreach (var bld in columns)
            {
                //special handling of dynamic types which are represented as BinaryData here
                var data = row[bld.Index];
                if (bld.UnderlyingType == typeof(JsonNode) && data is Newtonsoft.Json.Linq.JToken obj) 
                {
                    try
                    {

                        var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        data = System.Text.Json.JsonSerializer.Deserialize<JsonNode>(str);
                    }
                    catch
                    {
                        data = null;
                    }
                }

                if (data is DBNull n)
                {
                    data = null;
                }

                columnBuilders[bld.Index].Add(data);
            }
        }

        var rowCount = columnBuilders[0].RowCount;

        var builder = TableBuilder.CreateEmpty("logs", rowCount);
        foreach (var b in columnBuilders)
            builder.WithColumn(b.Name, b.ToColumn());

        var table = builder.ToTableSource();
        return table;

        Type BodgeGetFieldType(int n)
        {
            var t = reader.GetFieldType(n);
            if (t == typeof(object))
                t = typeof(JsonNode);
            return t;
        }
    }
}
