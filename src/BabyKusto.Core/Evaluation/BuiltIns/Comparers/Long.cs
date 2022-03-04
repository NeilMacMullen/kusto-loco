// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class LongAscComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((long)a).CompareTo((long)b);
        }
    }

    internal class LongDescComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((long)b).CompareTo((long)a);
        }
    }
}
