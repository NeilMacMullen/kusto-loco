using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class ToStringFromDateTimeFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Impl(DateTime input)
        => input.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
}