// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class StringAscComparer : IComparer
    {
        public int Compare(object a, object b) => StringComparer.Ordinal.Compare((string)a, (string)b);
    }

    internal class StringDescComparer : IComparer
    {
        public int Compare(object a, object b) => StringComparer.Ordinal.Compare((string)b, (string)a);
    }
}
