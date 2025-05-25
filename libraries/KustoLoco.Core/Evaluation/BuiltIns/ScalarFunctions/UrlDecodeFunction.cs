using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UrlDecode")]
internal partial class UrlDecodeFunction
{
    private static string Impl(string url) =>
        string.IsNullOrEmpty(url)
            ? string.Empty
            : Uri.UnescapeDataString(url);
}
