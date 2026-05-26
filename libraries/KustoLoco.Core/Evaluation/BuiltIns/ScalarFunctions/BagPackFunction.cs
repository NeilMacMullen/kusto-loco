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

internal class BagPackFunctionImpl : IScalarFunctionImpl
{
    private const int MaxPairs = 32;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var bag = new JsonObject();
        for (var i = 0; i + 1 < arguments.Length; i += 2)
        {
            if (arguments[i].Value is not string key || string.IsNullOrEmpty(key))
                continue;
            bag[key] = ToJsonNode(arguments[i + 1].Value);
        }
        return new ScalarResult(ScalarTypes.Dynamic, (JsonNode?)bag);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var rowCount = arguments[0].Column.RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
        {
            var bag = new JsonObject();
            for (var i = 0; i + 1 < arguments.Length; i += 2)
            {
                if (arguments[i].Column.GetRawDataValue(row) is not string key || string.IsNullOrEmpty(key))
                    continue;
                bag[key] = ToJsonNode(arguments[i + 1].Column.GetRawDataValue(row));
            }
            data[row] = bag;
        }
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

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
        var overloads = Enumerable.Range(1, MaxPairs)
            .Select(BuildOverloadForPairCount)
            .ToArray();
        var info = new ScalarFunctionInfo(overloads);
        functions.Add(Functions.BagPack, info);
        functions.Add(Functions.Pack, info); // pack() is a deprecated alias for bag_pack()
    }

    private static ScalarOverloadInfo BuildOverloadForPairCount(int pairCount)
    {
        var parameterTypes = Enumerable.Range(0, pairCount)
            .SelectMany(_ => new[] { ScalarTypes.String, ScalarTypes.Unknown })
            .ToArray();
        return new ScalarOverloadInfo(new BagPackFunctionImpl(), ScalarTypes.Dynamic, parameterTypes);
    }
}
