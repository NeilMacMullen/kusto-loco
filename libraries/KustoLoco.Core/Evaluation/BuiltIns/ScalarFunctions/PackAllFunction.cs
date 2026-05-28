//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class PackAllFunctionImpl : IScalarFunctionImpl
{
    private const int MaxColumns = 128;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var ignoreNullEmpty = arguments.Length > 0 && arguments[0].Value is bool b && b;
        var bag = new JsonObject();
        for (var i = 1; i + 1 < arguments.Length; i += 2)
        {
            if (arguments[i].Value is not string key) continue;
            var value = arguments[i + 1].Value;
            if (ignoreNullEmpty && IsNullOrEmpty(value)) continue;
            bag[key] = ToJsonNode(value);
        }
        return new ScalarResult(ScalarTypes.Dynamic, (JsonNode?)bag);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var rowCount = arguments[0].Column.RowCount;
        var ignoreNullEmpty = rowCount > 0 && arguments[0].Column.GetRawDataValue(0) is bool b && b;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
        {
            var bag = new JsonObject();
            for (var i = 1; i + 1 < arguments.Length; i += 2)
            {
                if (arguments[i].Column.GetRawDataValue(row) is not string key) continue;
                var value = arguments[i + 1].Column.GetRawDataValue(row);
                if (ignoreNullEmpty && IsNullOrEmpty(value)) continue;
                bag[key] = ToJsonNode(value);
            }
            data[row] = bag;
        }
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    private static bool IsNullOrEmpty(object? value) => value switch
    {
        null => true,
        string s => s.Length == 0,
        JsonObject obj => obj.Count == 0,
        JsonArray arr => arr.Count == 0,
        _ => false
    };

    private static JsonNode? ToJsonNode(object? value) => value switch
    {
        null => null,
        JsonNode node => node.DeepClone(),
        string s => JsonValue.Create(s),
        int i => JsonValue.Create(i),
        long l => JsonValue.Create(l),
        double d => JsonValue.Create(d),
        decimal dec => JsonValue.Create(dec),
        bool b => JsonValue.Create(b),
        DateTime dt => JsonValue.Create(dt),
        TimeSpan ts => JsonValue.Create(ts.ToString()),
        Guid g => JsonValue.Create(g),
        _ => JsonValue.Create(value.ToString())
    };

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions)
    {
        var overloads = Enumerable.Range(0, MaxColumns + 1)
            .Select(BuildOverloadForColumnCount)
            .ToArray();
        functions.Add(Functions.PackAll, new ScalarFunctionInfo(overloads));
    }

    private static ScalarOverloadInfo BuildOverloadForColumnCount(int columnCount)
    {
        var parameterTypes = new TypeSymbol[1 + columnCount * 2];
        parameterTypes[0] = ScalarTypes.Bool;
        for (var i = 0; i < columnCount; i++)
        {
            parameterTypes[1 + i * 2] = ScalarTypes.String;
            parameterTypes[2 + i * 2] = ScalarTypes.Unknown;
        }
        return new ScalarOverloadInfo(new PackAllFunctionImpl(), ScalarTypes.Dynamic, parameterTypes);
    }
}
