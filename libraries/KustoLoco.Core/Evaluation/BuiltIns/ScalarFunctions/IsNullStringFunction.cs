﻿

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class IsNullStringFunction
{
    /// <summary>
    ///     strings are never null
    /// </summary>
    internal static bool Impl(string s) => false;
}
