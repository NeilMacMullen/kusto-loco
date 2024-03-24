// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns;

public class ScalarFunctionInfo
{
    public ScalarFunctionInfo(params ScalarOverloadInfo[] overloads) => Overloads = overloads;

    public IReadOnlyList<ScalarOverloadInfo> Overloads { get; }
}