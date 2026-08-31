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

// set_union / set_intersect / set_difference take an arbitrary number of dynamic arrays, so they are registered
// manually (one overload per argument count) rather than via the source generator. All three deduplicate by JSON
// value-equality (canonical JSON text) and preserve first-occurrence order, matching ADX.
internal static class SetOpsSupport
{
    private const int MinSets = 2;
    private const int MaxSets = 64;

    private static IEnumerable<JsonNode?> Elements(object? value) =>
        value is JsonArray a ? a : Enumerable.Empty<JsonNode?>();

    private static string Key(JsonNode? node) => node?.ToJsonString() ?? "null";

    public static JsonNode Union(IReadOnlyList<object?> values)
    {
        var seen = new HashSet<string>();
        var result = new JsonArray();
        foreach (var v in values)
            foreach (var el in Elements(v))
                if (seen.Add(Key(el)))
                    result.Add(el?.DeepClone());
        return result;
    }

    public static JsonNode Intersect(IReadOnlyList<object?> values)
    {
        var others = new List<HashSet<string>>();
        for (var i = 1; i < values.Count; i++)
            others.Add(Elements(values[i]).Select(Key).ToHashSet());
        var seen = new HashSet<string>();
        var result = new JsonArray();
        foreach (var el in Elements(values.Count > 0 ? values[0] : null))
        {
            var key = Key(el);
            if (!seen.Add(key)) continue;
            if (others.All(s => s.Contains(key))) result.Add(el?.DeepClone());
        }
        return result;
    }

    public static JsonNode Difference(IReadOnlyList<object?> values)
    {
        var exclude = new HashSet<string>();
        for (var i = 1; i < values.Count; i++)
            foreach (var el in Elements(values[i]))
                exclude.Add(Key(el));
        var seen = new HashSet<string>();
        var result = new JsonArray();
        foreach (var el in Elements(values.Count > 0 ? values[0] : null))
        {
            var key = Key(el);
            if (!seen.Add(key)) continue;
            if (!exclude.Contains(key)) result.Add(el?.DeepClone());
        }
        return result;
    }

    public static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions,
        FunctionSymbol symbol, IScalarFunctionImpl impl)
    {
        var overloads = Enumerable.Range(MinSets, MaxSets - MinSets + 1)
            .Select(n => new ScalarOverloadInfo(impl, ScalarTypes.Dynamic,
                Enumerable.Repeat(ScalarTypes.Dynamic, n).ToArray()))
            .ToArray();
        functions.Add(symbol, new ScalarFunctionInfo(overloads));
    }
}

internal class SetUnionFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) =>
        new(ScalarTypes.Dynamic, SetOpsSupport.Union(arguments.Select(a => a.Value).ToArray()));

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments) => SetColumnar.Apply(arguments, SetOpsSupport.Union);

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> f) =>
        SetOpsSupport.Register(f, Functions.SetUnion, new SetUnionFunctionImpl());
}

internal class SetIntersectFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) =>
        new(ScalarTypes.Dynamic, SetOpsSupport.Intersect(arguments.Select(a => a.Value).ToArray()));

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments) => SetColumnar.Apply(arguments, SetOpsSupport.Intersect);

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> f) =>
        SetOpsSupport.Register(f, Functions.SetIntersect, new SetIntersectFunctionImpl());
}

internal class SetDifferenceFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) =>
        new(ScalarTypes.Dynamic, SetOpsSupport.Difference(arguments.Select(a => a.Value).ToArray()));

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments) => SetColumnar.Apply(arguments, SetOpsSupport.Difference);

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> f) =>
        SetOpsSupport.Register(f, Functions.SetDifference, new SetDifferenceFunctionImpl());
}

internal static class SetColumnar
{
    public static ColumnarResult Apply(ColumnarResult[] arguments,
        System.Func<IReadOnlyList<object?>, JsonNode> op)
    {
        var rowCount = arguments[0].Column.RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
            data[row] = op(arguments.Select(a => a.Column.GetRawDataValue(row)).ToArray());
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }
}
