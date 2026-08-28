//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Nodes;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// parse_user_agent(user_agent, look_for) -> dynamic with the requested component(s): "browser" -> { Browser: {...} },
// "os" -> { OperatingSystem: {...} }, "device" -> { Device: {...} }. look_for is one of those strings or a dynamic
// array of them. Backed by the host-registered IUserAgentParser (via EvaluationContext.Providers); the engine ships no
// user-agent database. No parser or a null user agent yields null.
internal class ParseUserAgentFunctionImpl : IScalarFunctionImpl, IContextualScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => InvokeScalar(arguments, default);

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments) => InvokeColumnar(arguments, default);

    public ScalarResult InvokeScalar(ScalarResult[] arguments, EvaluationContext context)
    {
        var parser = context.Providers?.Get<IUserAgentParser>();
        return new ScalarResult(ScalarTypes.Dynamic,
            Parse(parser, arguments[0].Value as string, arguments[1].Value));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments, EvaluationContext context)
    {
        var parser = context.Providers?.Get<IUserAgentParser>();
        var rowCount = arguments[0].Column.RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
            data[row] = Parse(parser, arguments[0].Column.GetRawDataValue(row) as string,
                arguments[1].Column.GetRawDataValue(row));
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    private static JsonNode? Parse(IUserAgentParser? parser, string? ua, object? lookFor)
    {
        if (parser is null || ua is null) return null;
        var info = parser.Parse(ua);
        var wanted = LookForSet(lookFor);
        var obj = new JsonObject();
        if (wanted.Contains("browser")) obj["Browser"] = Software(info.Browser, includePatchMinor: false);
        if (wanted.Contains("os")) obj["OperatingSystem"] = Software(info.OperatingSystem, includePatchMinor: true);
        if (wanted.Contains("device")) obj["Device"] = Device(info.Device);
        return obj;
    }

    private static HashSet<string> LookForSet(object? lookFor)
    {
        var set = new HashSet<string>();
        void Add(string? s) { if (!string.IsNullOrEmpty(s)) set.Add(Normalize(s)); }
        switch (lookFor)
        {
            case string s: Add(s); break;
            case JsonArray arr:
                foreach (var el in arr) Add(el?.ToString());
                break;
            case JsonValue v when v.TryGetValue<string>(out var vs): Add(vs); break;
        }
        return set;
    }

    private static string Normalize(string s) => s.Trim().ToLowerInvariant() switch
    {
        "os" or "operatingsystem" or "operating_system" => "os",
        _ => s.Trim().ToLowerInvariant()
    };

    private static JsonObject Software(UserAgentSoftware s, bool includePatchMinor)
    {
        var o = new JsonObject
        {
            ["Family"] = s.Family,
            ["Major"] = s.Major,
            ["Minor"] = s.Minor,
            ["Patch"] = s.Patch,
        };
        if (includePatchMinor) o["PatchMinor"] = s.PatchMinor;
        return o;
    }

    private static JsonObject Device(UserAgentDevice d) => new()
    {
        ["Family"] = d.Family,
        ["Brand"] = d.Brand,
        ["Model"] = d.Model,
    };

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions) =>
        functions.Add(Functions.ParseUserAgent, new ScalarFunctionInfo(
            new ScalarOverloadInfo(new ParseUserAgentFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.String, ScalarTypes.String),
            new ScalarOverloadInfo(new ParseUserAgentFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.String, ScalarTypes.Dynamic)));
}
