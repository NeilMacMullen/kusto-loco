// ReSharper disable PartialTypeWithSinglePart

namespace CustomFunctions;

[KustoImplementation(Keyword = "fizz")]
public partial class FizzFunction
{
    public static string Impl(long n)
    {
        var fizz = n % 3 == 0 ? "fizz" : string.Empty;
        var buzz = n % 5 == 0 ? "buzz" : string.Empty;
        return fizz + buzz;
    }
}