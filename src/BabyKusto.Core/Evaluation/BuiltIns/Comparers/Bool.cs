// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class BoolAscNullsFirstComparer : IComparer
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
                        : ((bool)a).CompareTo((bool)b);
        }
    }

    internal class BoolAscNullsLastComparer : IComparer
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
                        : ((bool)a).CompareTo((bool)b);
        }
    }

    internal class BoolDescNullsFirstComparer : IComparer
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
                        : ((bool)b).CompareTo((bool)a);
        }
    }

    internal class BoolDescNullsLastComparer : IComparer
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
                        : ((bool)b).CompareTo((bool)a);
        }
    }
}
