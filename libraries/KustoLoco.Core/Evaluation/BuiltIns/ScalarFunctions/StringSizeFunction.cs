using System.Text;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StringSize")]
internal partial class StringSizeFunction
{
    public long Impl(string s) => Encoding.UTF8.GetBytes(s).Length;
}
