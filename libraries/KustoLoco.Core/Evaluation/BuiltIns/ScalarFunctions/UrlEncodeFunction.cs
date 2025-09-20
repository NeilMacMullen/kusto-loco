using System.Web;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UrlEncode")]
internal partial class UrlEncodeFunction
{
    public string Impl(string s) => HttpUtility.UrlEncode(s);
}
