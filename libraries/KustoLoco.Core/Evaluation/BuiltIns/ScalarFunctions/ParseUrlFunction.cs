using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// parse_url(url) -> a dynamic object with the URL's Scheme, Host, Port, Path, Username, Password, Query Parameters and
// Fragment components (empty strings / an empty object for absent parts), matching ADX.
[KustoImplementation(Keyword = "Functions.ParseUrl")]
internal partial class ParseUrlFunction
{
    private static JsonNode? Impl(string url) => UrlSupport.ParseUrl(url);
}
