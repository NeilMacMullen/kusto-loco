// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class IntAscComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((int)a).CompareTo((int)b);
        }
    }

    internal class IntDescComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((int)b).CompareTo((int)a);
        }
    }
}
