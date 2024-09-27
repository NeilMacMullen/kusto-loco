namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "padleft")]
internal partial class PadLeftFunction
{
    private static string Impl(string s,long n) => s.PadLeft((int)n,' ');
    private static string WithCharImpl(string s,long n,string pad) => s.PadLeft((int)n,pad.Length >0 ? pad[0]:' ');
}
