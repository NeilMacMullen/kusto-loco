using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class IsNullStringFunction
{
    /// <summary>
    ///     strings are never null
    /// </summary>
    internal static bool Impl(string s) => false;
}