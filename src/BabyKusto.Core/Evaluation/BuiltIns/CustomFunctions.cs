using System.Collections.Generic;
using BabyKusto.Core.Evaluation.BuiltIns.Impl;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns;

internal static class CustomFunctions
{
    internal static readonly Dictionary<FunctionSymbol, ScalarFunctionInfo> functions = new();

    static CustomFunctions()
    {
        functions.Add(DebugEmit, new ScalarFunctionInfo(new ScalarOverloadInfo(new DebugEmitImpl(),
            ScalarTypes.Int,
            ScalarTypes.String)));
        functions.Add(Levenshtein, new ScalarFunctionInfo(LevenshteinDistanceImpl.Overload));
        functions.Add(StringSimilarity, new ScalarFunctionInfo(StringSimilarityImpl.Overload));

        functions.Add(DateTimeToIso, new ScalarFunctionInfo(DateTimeToIsoImpl.Overload));

        functions.Add(TrimWs, new ScalarFunctionInfo(TrimWsFunctionImpl.Overload));
    }

    #region AdditionalFunctionSymbols

    public static readonly FunctionSymbol DebugEmit =
        new FunctionSymbol("debug_emit", ScalarTypes.Int,
                new Parameter("value1", ScalarTypes.String)
            ).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);


    public static readonly FunctionSymbol Levenshtein =
        new FunctionSymbol("levenshtein", ScalarTypes.Int, new Parameter("value1", ScalarTypes.String),
                new Parameter("value2", ScalarTypes.String)).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol StringSimilarity =
        new FunctionSymbol("string_similarity", ScalarTypes.Real, new Parameter("value1", ScalarTypes.String),
                new Parameter("value2", ScalarTypes.String)).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol DateTimeToIso =
        new FunctionSymbol("datetime_to_iso", ScalarTypes.String, new Parameter("value1", ScalarTypes.DateTime))
            .ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol TrimWs =
        new FunctionSymbol("trimws", ScalarTypes.String, new Parameter("value1", ScalarTypes.String))
            .ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);

    public static readonly FunctionSymbol[] AdditionalFunctionSymbols =
        { DebugEmit, Levenshtein, StringSimilarity, DateTimeToIso, TrimWs };

    #endregion
}