// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal sealed class AggregateOverloadInfo : OverloadInfoBase
    {
        public AggregateOverloadInfo(IAggregateImpl aggregateImpl, TypeSymbol returnType, params TypeSymbol[] parameterTypes)
           : base(returnType, parameterTypes)
        {
            AggregateImpl = aggregateImpl;
        }

        public IAggregateImpl AggregateImpl { get; }
    }
}
