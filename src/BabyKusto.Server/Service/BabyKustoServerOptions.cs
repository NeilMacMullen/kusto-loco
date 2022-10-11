// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace BabyKusto.Server.Service
{
    public class BabyKustoServerOptions
    {
        public string DatabaseName { get; set; } = "BabyKusto";
        public string DatabaseId { get; set; } = Guid.NewGuid().ToString();
    }
}
