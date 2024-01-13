// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using BabyKusto.Core.Util;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation]
    internal class DateTimeToIso
    {
        private static string Impl(DateTime input)
            => input.ToString("o", CultureInfo.InvariantCulture);
    }
}