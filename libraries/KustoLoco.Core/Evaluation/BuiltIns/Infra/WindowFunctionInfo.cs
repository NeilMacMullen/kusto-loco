//
// Licensed under the MIT License.

using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal class WindowFunctionInfo
{
    public WindowFunctionInfo(params WindowOverloadInfo[] overloads) => Overloads = overloads;

    public IReadOnlyList<WindowOverloadInfo> Overloads { get; }
}