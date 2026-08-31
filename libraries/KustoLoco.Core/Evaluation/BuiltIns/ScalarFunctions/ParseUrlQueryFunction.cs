using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// parse_urlquery(query) -> a dynamic object { "Query Parameters": { key: value, ... } } from a URL query string.
[KustoImplementation(Keyword = "Functions.ParseUrlQuery")]
internal partial class ParseUrlQueryFunction
{
    private static JsonNode? Impl(string query) => new JsonObject { ["Query Parameters"] = UrlSupport.ParseQuery(query) };
}
