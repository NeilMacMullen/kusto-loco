// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DateTimeAscNullsFirstComparer : IComparer
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
                        : ((DateTime)a).CompareTo((DateTime)b);
        }
    }

    internal class DateTimeAscNullsLastComparer : IComparer
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
                        : ((DateTime)a).CompareTo((DateTime)b);
        }
    }

    internal class DateTimeDescNullsFirstComparer : IComparer
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
                        : ((DateTime)b).CompareTo((DateTime)a);
        }
    }

    internal class DateTimeDescNullsLastComparer : IComparer
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
                        : ((DateTime)b).CompareTo((DateTime)a);
        }
    }
}
