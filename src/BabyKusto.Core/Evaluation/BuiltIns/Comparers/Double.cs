// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DoubleAscComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((double)a).CompareTo((double)b);
        }
    }

    internal class DoubleDescComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((double)b).CompareTo((double)a);
        }
    }
}
