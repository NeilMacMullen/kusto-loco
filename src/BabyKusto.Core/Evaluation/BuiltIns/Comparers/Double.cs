// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DoubleAscNullsFirstComparer : IComparer
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
                        : ((double)a).CompareTo((double)b);
        }
    }

    internal class DoubleAscNullsLastComparer : IComparer
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
                        : ((double)a).CompareTo((double)b);
        }
    }

    internal class DoubleDescNullsFirstComparer : IComparer
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
                        : ((double)b).CompareTo((double)a);
        }
    }

    internal class DoubleDescNullsLastComparer : IComparer
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
                        : ((double)b).CompareTo((double)a);
        }
    }
}
