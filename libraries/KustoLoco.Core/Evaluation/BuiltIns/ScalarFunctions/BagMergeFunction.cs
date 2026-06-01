//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class BagMergeFunctionImpl : IScalarFunctionImpl
{
    private const int MinBags = 2;
    private const int MaxBags = 64;

    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var bag = new JsonObject();
        foreach (var argument in arguments)
            MergeInto(bag, argument.Value);
        return new ScalarResult(ScalarTypes.Dynamic, (JsonNode?)bag);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var rowCount = arguments[0].Column.RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
        {
            var bag = new JsonObject();
            foreach (var argument in arguments)
                MergeInto(bag, argument.Column.GetRawDataValue(row));
            data[row] = bag;
        }
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    // Leftmost-arg wins on key collision (matches ADX bag_merge and the make_bag aggregate).
    private static void MergeInto(JsonObject target, object? value)
    {
        if (value is not JsonObject source) return;
        foreach (var kvp in source)
        {
            if (target.ContainsKey(kvp.Key)) continue;
            target[kvp.Key] = kvp.Value?.DeepClone();
        }
    }

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions)
    {
        var overloads = Enumerable.Range(MinBags, MaxBags - MinBags + 1)
            .Select(BuildOverloadForBagCount)
            .ToArray();
        functions.Add(Functions.BagMerge, new ScalarFunctionInfo(overloads));
    }

    private static ScalarOverloadInfo BuildOverloadForBagCount(int bagCount)
    {
        var parameterTypes = Enumerable.Repeat(ScalarTypes.Dynamic, bagCount).ToArray();
        return new ScalarOverloadInfo(new BagMergeFunctionImpl(), ScalarTypes.Dynamic, parameterTypes);
    }
}
