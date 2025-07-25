// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal sealed class AggregateOverloadInfo : OverloadInfoBase
{
    public AggregateOverloadInfo(IAggregateImpl aggregateImpl, TypeSymbol returnType,
        params TypeSymbol[] parameterTypes)
        : base(returnType, parameterTypes)
    {
        AggregateImpl = aggregateImpl;
    }

    public AggregateOverloadInfo(IAggregateImpl aggregateImpl, TypeSymbol returnType,int numParametersToMatch,
        params TypeSymbol[] parameterTypes)
        : base(returnType, numParametersToMatch,parameterTypes)
    {
        AggregateImpl = aggregateImpl;
    }

    public IAggregateImpl AggregateImpl { get; }
}
