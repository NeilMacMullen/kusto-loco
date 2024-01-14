// ReSharper disable PartialTypeWithSinglePart

namespace MyNamespace;

[KustoImplementation(Keyword = "Functions.Strlen")]
public partial class FizzFunction
{
    public static string Impl(long n) => "bzzz";
}