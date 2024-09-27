using System.Collections.Generic;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal static class CustomFunctions
{
    internal static readonly Dictionary<FunctionSymbol, ScalarFunctionInfo> functions = new();

    #region AdditionalFunctionSymbols

    public static readonly FunctionSymbol DebugEmit =
        new FunctionSymbol("debug_emit", ScalarTypes.Int,
                new Parameter("value1", ScalarTypes.String)
            ).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    #endregion

    static CustomFunctions()
    {
        functions.Add(DebugEmit, new ScalarFunctionInfo(new ScalarOverloadInfo(new DebugEmitImpl(),
            ScalarTypes.Int,
            ScalarTypes.String)));
        LevenshteinDistance.Register(functions);
        StringSimilarity.Register(functions);
        DateTimeToIso.Register(functions);
        TrimWsFunction.Register(functions);
        PadLeftFunction.Register(functions);
        PadRightFunction.Register(functions);
        ToDateTimeFmtFunction.Register(functions);
        ParseHexFunction.Register(functions);
    }
}
