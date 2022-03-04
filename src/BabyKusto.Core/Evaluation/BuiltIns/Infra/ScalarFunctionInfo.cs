// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal class ScalarFunctionInfo
    {
        public ScalarFunctionInfo(params ScalarOverloadInfo[] overloads)
        {
            Overloads = overloads;
        }

        public IReadOnlyList<ScalarOverloadInfo> Overloads { get; }
    }
}
