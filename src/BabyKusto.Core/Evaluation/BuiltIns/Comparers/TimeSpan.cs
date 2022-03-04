// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class TimeSpanAscNullsFirstComparer : IComparer
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
                        : ((TimeSpan)a).CompareTo((TimeSpan)b);
        }
    }

    internal class TimeSpanAscNullsLastComparer : IComparer
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
                        : ((TimeSpan)a).CompareTo((TimeSpan)b);
        }
    }

    internal class TimeSpanDescNullsFirstComparer : IComparer
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
                        : ((TimeSpan)b).CompareTo((TimeSpan)a);
        }
    }

    internal class TimeSpanDescNullsLastComparer : IComparer
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
                        : ((TimeSpan)b).CompareTo((TimeSpan)a);
        }
    }
}
