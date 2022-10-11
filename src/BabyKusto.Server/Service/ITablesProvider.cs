// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using BabyKusto.Core;

namespace BabyKusto.Server.Service
{
    public interface ITablesProvider
    {
        List<ITableSource> GetTables();
    }
}
