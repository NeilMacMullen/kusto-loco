// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns;

public sealed class ScalarOverloadInfo : OverloadInfoBase
{
    public EvaluationHints EvaluationHints;

    public ScalarOverloadInfo(IScalarFunctionImpl impl, TypeSymbol returnType, params TypeSymbol[] parameterTypes)
        : this(impl, EvaluationHints.None, returnType, parameterTypes)
    {
    }

    public ScalarOverloadInfo(IScalarFunctionImpl impl, EvaluationHints hints, TypeSymbol returnType,
        params TypeSymbol[] parameterTypes)
        : base(returnType, parameterTypes)
    {
        ScalarImpl = impl;
        EvaluationHints = hints;
    }

    public IScalarFunctionImpl ScalarImpl { get; }
}