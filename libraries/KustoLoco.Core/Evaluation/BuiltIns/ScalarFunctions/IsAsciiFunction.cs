using System.Linq;
using System.Text;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;
// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.IsAscii")]
internal partial class IsAsciiFunction
{
    internal static bool Impl(string s) => Ascii.IsValid(s);
}


// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.Reverse")]
internal partial class ReverseFunction
{
    internal static string Impl(string s) => new(s.Reverse().ToArray());
}

[KustoImplementation(Keyword = "Functions.IsUtf8")]
internal partial class IsUtf8Function
{
    //it'a a little unclear how this could ever return false.
    //In theory we can have some bytes that are not valid utf8,
    //but how would the get turned into a string in the tables?
    internal static bool Impl(string s) => true;
}
