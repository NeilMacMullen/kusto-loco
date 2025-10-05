namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "format_interp")]
internal partial class FormatInterpFunction
{
    private static string AImpl(string fmt, object a) => string.Format(fmt, a);
    private static string AbImpl(string fmt, object a, object b) => string.Format(fmt, a, b);
    private static string AbcImpl(string fmt, object a, object b,object c) => string.Format(fmt, a, b, c);
    private static string AbcdImpl(string fmt, object a, object b, object c,object d) => string.Format(fmt, a, b, c, d);
    private static string AbcdeImpl(string fmt, object a, object b, object c, object d, object e) => string.Format(fmt, a, b, c, d, e);
    private static string AbcdefImpl(string fmt, object a, object b, object c, object d, object e,object f) => string.Format(fmt, a, b, c, d, e, f);
    private static string AbcdefgImpl(string fmt, object a, object b, object c, object d, object e, object f,object g) => string.Format(fmt, a, b, c, d, e, f, g);
    private static string AbcdefghImpl(string fmt, object a, object b, object c, object d, object e, object f, object g,object h) => string.Format(fmt, a, b, c, d, e, f, g,h);
    private static string AbcdefghiImpl(string fmt, object a, object b, object c, object d, object e, object f, object g, object h,object x) => string.Format(fmt, a, b, c, d, e, f, g, h,x);
    private static string AbcdefghijImpl(string fmt, object a, object b, object c, object d, object e, object f, object g, object h, object x,object y)
        => string.Format(fmt, a, b, c, d, e, f, g, h, x,y);

}
