// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal class WindowFunctionInfo
    {
        public WindowFunctionInfo(params WindowOverloadInfo[] overloads)
        {
            Overloads = overloads;
        }

        public IReadOnlyList<WindowOverloadInfo> Overloads { get; }
    }
}
