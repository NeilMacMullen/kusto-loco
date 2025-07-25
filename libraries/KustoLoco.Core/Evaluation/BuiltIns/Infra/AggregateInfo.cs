// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal class AggregateInfo
{
    public AggregateInfo(params AggregateOverloadInfo[] overloads) => Overloads = overloads;


    public IReadOnlyList<AggregateOverloadInfo> Overloads { get; }
}
