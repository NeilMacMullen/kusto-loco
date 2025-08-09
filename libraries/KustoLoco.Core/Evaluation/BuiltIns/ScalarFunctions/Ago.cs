//
// Licensed under the MIT License.
// ReSharper disable PartialTypeWithSinglePart
using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Ago")]
internal partial class AgoFunction
{
    private static DateTime Impl(TimeSpan t) => DateTime.UtcNow - t;
}