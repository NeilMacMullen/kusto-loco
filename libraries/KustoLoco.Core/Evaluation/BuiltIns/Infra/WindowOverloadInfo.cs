﻿//
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns;

internal sealed class WindowOverloadInfo : OverloadInfoBase
{
    public WindowOverloadInfo(IWindowFunctionImpl impl, TypeSymbol returnType, params TypeSymbol[] parameterTypes)
        : base(returnType, parameterTypes) =>
        Impl = impl;

    public IWindowFunctionImpl Impl { get; }
}