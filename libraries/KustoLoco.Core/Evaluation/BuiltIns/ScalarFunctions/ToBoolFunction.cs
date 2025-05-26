namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToBool")]
internal partial class ToBoolFunction
{
    private static bool IntImpl(int n) => n != 0;
    private static bool DecImpl(int n) => n != 0;
    private static bool LongImpl(long n) => n != 0;
    private static bool BoolImpl(bool b) => b;
    private static bool DoubleImpl(double f) => f != 0;

    private static bool? StringImpl(string input) =>
        bool.TryParse(input, out var parsedResult)
            ? parsedResult
            : long.TryParse(input, out var parsedLong)
                ? parsedLong != 0
                : null;
}
