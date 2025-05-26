using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UrlEncode_Component")]
internal partial class UrlEncodeComponentFunction
{
    private static string Impl(string url) =>
        string.IsNullOrEmpty(url)
            ? string.Empty
            : Uri.EscapeDataString(url);
}
