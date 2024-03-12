// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ReSharper disable PartialTypeWithSinglePart
using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.FormatDatetime")]
internal partial class FormatDateTime
{
    private static string Impl(DateTime date,string format) => date.ToString(format);
}