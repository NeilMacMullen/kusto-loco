using System.Diagnostics;
using System.Text.RegularExpressions;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

/// <summary>
///     Provides core functionality for kusto Trim operations
/// </summary>
/// <remarks>
///     Different flavours of the command trim either the start, end or both ends
///     of the string.
///     Kusto uses a slightly different regex engine to .Net so results will not
///     be completely consist with "real" kusto
/// </remarks>
internal abstract class TrimFunctionCore : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);

        return new ScalarResult(ScalarTypes.String, Trim(arguments[0].Value, arguments[1].Value));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);

        var columns = new Column<string?>[arguments.Length];
        for (var i = 0; i < arguments.Length; i++)
        {
            columns[i] = (Column<string?>)arguments[i].Column;
        }

        var rowCount = columns[0].RowCount;
        var data = new string?[rowCount];
        for (var i = 0; i < rowCount; i++)
        {
            data[i] = Trim(columns[0][i], columns[1][i]);
        }

        return new ColumnarResult(BaseColumn.Create(ScalarTypes.String, data));
    }

    protected abstract string Trim(object? regex, object? target);

    protected string Evaluate(object? regex, object? target, bool trimStart, bool trimEnd)
    {
        if (target is not string targetString || regex is not string regexString)
            return string.Empty;
        if (trimStart) targetString = Regex.Replace(targetString, "^" + regexString, string.Empty);
        if (trimEnd) targetString = Regex.Replace(targetString, regexString + "$", string.Empty);
        return targetString;
    }
}

internal class TrimFunctionImpl : TrimFunctionCore
{
    protected override string Trim(object? regex, object? target) => Evaluate(regex, target, true, true);
}

internal class TrimStartFunctionImpl : TrimFunctionCore
{
    protected override string Trim(object? regex, object? target) => Evaluate(regex, target, true, false);
}

internal class TrimEndFunctionImpl : TrimFunctionCore
{
    protected override string Trim(object? regex, object? target) => Evaluate(regex, target, false, true);
}