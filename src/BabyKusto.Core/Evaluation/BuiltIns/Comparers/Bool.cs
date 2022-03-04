// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class BoolAscComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((bool)a).CompareTo((bool)b);
        }
    }

    internal class BoolDescComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            return ((bool)b).CompareTo((bool)a);
        }
    }
}
