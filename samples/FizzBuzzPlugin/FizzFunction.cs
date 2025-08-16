using KustoLoco.SourceGeneration.Attributes;

namespace FizzBuzzPlugin;

[KustoImplementation(Keyword = "fizz")]
public partial class FizzFunction
{
    //note that the method will only ever be called on non-null values
    public static string Impl(long n)
    {
        var fizz = n % 3 == 0 ? "fizz" : string.Empty;
        var buzz = n % 5 == 0 ? "buzz" : string.Empty;
        return fizz + buzz;
    }
    public static string intImpl(int n)
    {
        var fizz = n % 3 == 0 ? "fizz" : string.Empty;
        var buzz = n % 5 == 0 ? "buzz" : string.Empty;
        return fizz + buzz;
    }

    public static string StringImpl(string text)
    {
        var n = text.Length;
        var fizz = n % 3 == 0 ? "fizz" : string.Empty;
        var buzz = n % 5 == 0 ? "buzz" : string.Empty;
        return fizz + buzz;
    }

}


