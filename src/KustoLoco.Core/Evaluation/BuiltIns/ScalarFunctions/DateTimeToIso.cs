// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ReSharper disable PartialTypeWithSinglePart

using System;
using System.Globalization;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation(Keyword = "datetime_to_iso")]
    internal partial class DateTimeToIso
    {
        private static string Impl(DateTime input)
            => input.ToString("o", CultureInfo.InvariantCulture);
    }
}