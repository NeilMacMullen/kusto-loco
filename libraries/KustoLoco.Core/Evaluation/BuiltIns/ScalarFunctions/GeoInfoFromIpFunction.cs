//
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net;
using System.Text.Json.Nodes;
using Kusto.Language;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// geo_info_from_ip_address(ip) -> dynamic { country, state, city, latitude, longitude } resolved by the
// host-registered IGeoIpProvider (read from EvaluationContext.Providers). No provider registered, an unparseable
// address, or an unresolved address all yield null, so geo predicates stay inert rather than failing the query. This
// is a context-aware scalar (IContextualScalarFunctionImpl): the engine ships no geo database, the host supplies it.
internal class GeoInfoFromIpFunctionImpl : IScalarFunctionImpl, IContextualScalarFunctionImpl
{
    // The context-free path is never taken (the evaluator routes context-aware impls to the context method); it exists
    // for interface completeness and degrades to "no provider" -> null.
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => InvokeScalar(arguments, default);

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments) => InvokeColumnar(arguments, default);

    public ScalarResult InvokeScalar(ScalarResult[] arguments, EvaluationContext context)
    {
        var provider = context.Providers?.Get<IGeoIpProvider>();
        return new ScalarResult(ScalarTypes.Dynamic, Lookup(provider, arguments[0].Value as string));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments, EvaluationContext context)
    {
        var provider = context.Providers?.Get<IGeoIpProvider>();
        var rowCount = arguments[0].Column.RowCount;
        var data = NullableSetBuilderOfJsonNode.CreateFixed(rowCount);
        for (var row = 0; row < rowCount; row++)
            data[row] = Lookup(provider, arguments[0].Column.GetRawDataValue(row) as string);
        return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
    }

    private static JsonNode? Lookup(IGeoIpProvider? provider, string? ip)
    {
        if (provider is null || string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out var address))
            return null;
        var info = provider.Lookup(address);
        if (info is null) return null;
        return new JsonObject
        {
            ["country"] = info.Country is null ? null : JsonValue.Create(info.Country),
            ["state"] = info.State is null ? null : JsonValue.Create(info.State),
            ["city"] = info.City is null ? null : JsonValue.Create(info.City),
            ["latitude"] = info.Latitude is null ? null : JsonValue.Create(info.Latitude.Value),
            ["longitude"] = info.Longitude is null ? null : JsonValue.Create(info.Longitude.Value),
        };
    }

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions) =>
        functions.Add(Functions.IpGeoLocation,
            new ScalarFunctionInfo(new ScalarOverloadInfo(
                new GeoInfoFromIpFunctionImpl(), ScalarTypes.Dynamic, ScalarTypes.String)));
}
