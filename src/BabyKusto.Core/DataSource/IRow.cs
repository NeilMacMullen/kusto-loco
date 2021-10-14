// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core
{
    public interface IRow
    {
        TableSchema Schema { get; }

        object?[] Values { get; }
    }
}
