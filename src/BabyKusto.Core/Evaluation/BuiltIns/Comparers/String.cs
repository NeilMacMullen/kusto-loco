// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class StringAscNullsFirstComparer : IComparer
    {
        public int Compare(object? a, object? b)
        {
            return
                (a == null && b == null)
                ? 0
                : a == null
                    ? 1
                    : b == null
                        ? -1
                        : StringComparer.Ordinal.Compare((string)a, (string)b);
        }
    }

    internal class StringAscNullsLastComparer : IComparer
    {
        public int Compare(object? a, object? b)
        {
            return
                (a == null && b == null)
                ? 0
                : a == null
                    ? -1
                    : b == null
                        ? 1
                        : StringComparer.Ordinal.Compare((string)a, (string)b);
        }
    }

    internal class StringDescNullsFirstComparer : IComparer
    {
        public int Compare(object? a, object? b)
        {
            return
                (a == null && b == null)
                ? 0
                : a == null
                    ? 1
                    : b == null
                        ? -1
                        : StringComparer.Ordinal.Compare((string)b, (string)a);
        }
    }

    internal class StringDescNullsLastComparer : IComparer
    {
        public int Compare(object? a, object? b)
        {
            return
                (a == null && b == null)
                ? 0
                : a == null
                    ? -1
                    : b == null
                        ? 1
                        : StringComparer.Ordinal.Compare((string)b, (string)a);
        }
    }
}
